using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using MyApp.Common;
using MyApp.Repositories.Interfaces;
using MyApp.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace MyApp.Services.Implementations
{
    public class GenericService<TEntity, TDto> : IGenericService<TEntity, TDto>
        where TEntity : BaseEntity
    {
        protected readonly IGenericRepository<TEntity> _repository;
        protected readonly IMapper _mapper;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public GenericService(
            IGenericRepository<TEntity> repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        // Get current logged-in user id from JWT token
        private int? GetCurrentUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst("userId")?.Value;
            if (int.TryParse(userId, out int id))
                return id;
            return null; // system user or null
        }

        public virtual async Task<IEnumerable<TDto>> GetAllAsync()
        {
            var entities = await _repository.Query()
                                            .Where(e => !e.IsDeleted)
                                            .ToListAsync();
            return _mapper.Map<IEnumerable<TDto>>(entities);
        }

        public virtual async Task<TDto?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted) return default;

            return _mapper.Map<TDto>(entity);
        }

        public virtual async Task<TDto> AddAsync(TDto dto)
        {
            var entity = _mapper.Map<TEntity>(dto);

            entity.CreatedOn = DateTime.UtcNow;
            entity.CreatedBy = GetCurrentUserId();

            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();

            return _mapper.Map<TDto>(entity);
        }

        public virtual async Task<TDto?> UpdateAsync(int id, TDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted) return default;

            _mapper.Map(dto, entity);

            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = GetCurrentUserId();

            await _repository.SaveChangesAsync();
            return _mapper.Map<TDto>(entity);
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted) return false;

            // Soft delete
            entity.IsDeleted = true;
            entity.DeletedOn = DateTime.UtcNow;
            entity.DeletedBy = GetCurrentUserId();

            await _repository.UpdateAsync(entity);
            await _repository.SaveChangesAsync();

            return true;
        }
    }
}
