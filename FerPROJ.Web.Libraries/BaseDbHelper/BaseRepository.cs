using FerPROJ.Libraries.DBHelper.Extensions;
using FerPROJ.Web.Libraries.BaseDataExtensions;
using FerPROJ.Web.Libraries.BaseDbExtensions;
using FerPROJ.Web.Libraries.BaseEntities;
using FerPROJ.Web.Libraries.BaseModels;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;

namespace FerPROJ.Web.Libraries.BaseDbHelper {
    public abstract class BaseRepository<TContext, TModel, TEntity> : IDisposable
        where TContext : BaseDbContext
        where TModel : BaseModel
        where TEntity : BaseEntity {

        public TContext _ts;
        protected BaseRepository(TContext ts) {
            _ts = ts;
        }
        public void Dispose() {
            throw new NotImplementedException();
        }

        #region Base GET for Model
        public virtual async Task<TModel> GetPrepareModelAsync(TModel model = null, string prefix = "FRM#") {
            if (model == null) {
                model = Activator.CreateInstance<TModel>();
            }
            return model;
        }
        public virtual async Task<TModel> GetPrepareModelByEntityAsync(TEntity entity) {
            return entity.ToDestination<TModel>();
        }
        public virtual async Task<TModel> GetPrepareModelByIdAsync(Guid id) {
            var entity = await GetByIdAsync(id);
            return await CacheExtensions.GetOrCreateCacheAsync($"GetEntity:{id}", async () => {
                return await GetPrepareModelByEntityAsync(entity);
            });
        }
        public virtual async Task<TModel> GetPrepareModelByPredicateAsync(Expression<Func<TEntity, bool>> predicate) {
            var entity = await GetByPredicateAsync(predicate);
            return await CacheExtensions.GetOrCreateCacheAsync($"GetEntity:{entity.GetPropertyValue<string>("Id")}", async () => {
                return await GetPrepareModelByEntityAsync(entity);
            });
        }
        #endregion

        #region Base GetDBEntity Method
        public async Task<int> GetCountAsync(Expression<Func<TEntity, bool>> whereCondition = null) {
            return await _ts.GetCountAsync(whereCondition);
        }
        public async Task<string> GetGeneratedIDAsync(string prefix, bool withSlash = true) {
            return await _ts.GetGeneratedIDAsync<TEntity>(prefix, withSlash);
        }

        public async Task<string> GetGeneratedIDAsync(string prefix, bool withSlash, Expression<Func<TEntity, bool>> whereCondition) {
            return await _ts.GetGeneratedIDAsync(prefix, withSlash, whereCondition);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync() {
            return await _ts.GetAllAsync<TEntity>();
        }

        protected virtual async Task<IEnumerable<TEntity>> GetAllWithSearchAsync(string searchText, DateTime? dateFrom, DateTime? dateTo, int dataLimit = int.MaxValue) {
            return await _ts.GetAllWithSearchAsync<TEntity>(searchText, dateFrom, dateTo, dataLimit);
        }
        public virtual async Task<IEnumerable<TModel>> GetViewModelWithSearchAsync(string searchText, DateTime? dateFrom, DateTime? dateTo, int dataLimit = int.MaxValue) {

            var query = await GetAllWithSearchAsync(null, dateFrom, dateTo);

            query = query.GetAllActiveOnly();

            query = query.OrderByProperty("DateCreated", false);

            var result = await query.SelectListAsync(async c => {

                return await CacheExtensions.GetOrCreateCacheAsync($"GetList:{c.GetPropertyValue<string>("Id")}", async () => {
                    return await GetPrepareModelByEntityAsync(c);
                });

            }, c => c.SearchForText(searchText), dataLimit);

            return result;
        }
        public virtual async Task<IEnumerable<TModel>> GetViewModelWithSearchAsync(Expression<Func<TEntity, bool>> whereCondition, string searchText, DateTime? dateFrom, DateTime? dateTo, int dataLimit = int.MaxValue) {

            var query = await GetAllAsync(whereCondition);

            query = query.GetAllActiveOnly();

            query = query.OrderByProperty("DateCreated", false);

            var result = await query.SelectListAsync(async c => {

                return await CacheExtensions.GetOrCreateCacheAsync($"GetList:{c.GetPropertyValue<string>("Id")}", async () => {
                    return await GetPrepareModelByEntityAsync(c);
                });

            }, c => c.SearchFor(searchText, dateFrom, dateTo, d => d.DateCreated), dataLimit);

            return result;
        }
        public virtual async Task<(IEnumerable<TModel> ModelItems, int TotalCount)> GetViewModelWithSearchAsync(string searchText, DateTime? dateFrom, DateTime? dateTo, int page, int dataLimit = int.MaxValue) {

            var query = await GetAllWithSearchAsync(null, dateFrom, dateTo, int.MaxValue);

            dataLimit = !searchText.IsNullOrEmpty() ||
                        !dateFrom.IsNullOrEmpty() ||
                        !dateTo.IsNullOrEmpty() ? int.MaxValue : dataLimit;

            query = query.GetAllActiveOnly();

            query = query.OrderByProperty("DateCreated", false);

            var result = await query.SelectListAsync(async c => {

                return await CacheExtensions.GetOrCreateCacheAsync($"GetList:{c.GetPropertyValue<string>("Id")}", async () => {
                    return await GetPrepareModelByEntityAsync(c);
                });

            }, c => c.SearchForText(searchText), page, dataLimit);

            return (result, query.Count());
        }
        public virtual async Task<(IEnumerable<TModel> ModelItems, int TotalCount)> GetViewModelWithSearchAsync(Expression<Func<TEntity, bool>> whereCondition, string searchText, DateTime? dateFrom, DateTime? dateTo, int page, int dataLimit = int.MaxValue) {

            var query = await GetAllAsync(whereCondition);

            query = query.GetAllActiveOnly();

            query = query.OrderByProperty("DateCreated", false);

            dataLimit = !searchText.IsNullOrEmpty() ||
                        !dateFrom.IsNullOrEmpty() ||
                        !dateTo.IsNullOrEmpty() ? int.MaxValue : dataLimit;

            var result = await query.SelectListAsync(async c => {

                return await CacheExtensions.GetOrCreateCacheAsync($"GetList:{c.GetPropertyValue<string>("Id")}", async () => {
                    return await GetPrepareModelByEntityAsync(c);
                });

            }, c => c.SearchFor(searchText, dateFrom, dateTo, d => d.DateCreated), page, dataLimit);

            return (result, query.Count());
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> whereCondition) {
            return await _ts.GetAllAsync(whereCondition);
        }

        public virtual async Task<TEntity> GetByIdAsync(Guid id) {
            return await _ts.GetByIdAsync<TEntity>(id);
        }

        public virtual async Task<TEntity> GetByPropertyAsync<TValueType>(TValueType propertyValue, string propertyName) {
            return await _ts.GetByIdAsync<TEntity, TValueType>(propertyValue, propertyName);
        }

        public virtual async Task<TEntity> GetByPredicateAsync(Expression<Func<TEntity, bool>> predicate) {
            return await _ts.GetByPredicateAsync(predicate);
        }
        #endregion


        #region Base DTO CRUD
        protected async virtual Task SaveDataAsync(TModel model) {
            model.Id = Guid.NewGuid();
            await _ts.SaveModelAndCommitAsync<TModel, TEntity>(model);
        }

        public async Task<bool> SaveModelAsync(TModel model) {
            if (model == null)
                throw new ArgumentNullException($"{nameof(model)} is null!");

            if (!model.DataValidation()) {
                var sb = new StringBuilder();
                if (!string.IsNullOrEmpty(model.Error))
                    sb.AppendLine("Error 1: " + model.Error);
                if (!string.IsNullOrEmpty(model.ErrorMessage))
                    sb.AppendLine("Error 2: " + model.ErrorMessage);
                if (model.ErrorMessages.Length > 0)
                    sb.AppendLine("Error 3: " + model.ErrorMessages.ToString());
                throw new ArgumentException(sb.ToString());
            }

            if (!model.Success)
                throw new ArgumentException(model.Error);

            try {
                await SaveDataAsync(model);
                return true;
            }
            catch (DbUpdateException ex) {
                var sb = new StringBuilder();
                var innerEx = ex.InnerException;
                int innerLevel = 1;

                while (innerEx != null) {
                    sb.AppendLine($"Inner Exception Level {innerLevel}: {innerEx.Message}\n");

                    if (innerEx is MySqlException mySqlEx)
                        sb.AppendLine($"SQL Error Code: {mySqlEx.Number}\n");

                    innerEx = innerEx.InnerException;

                    innerLevel++;
                }

                if (ex.Entries != null && ex.Entries.Any()) {

                    sb.AppendLine("\nEntities involved in the exception:");

                    foreach (var entry in ex.Entries)

                        sb.AppendLine($"TableName: {entry.Entity.GetType().Name}, Operation: {entry.State}");
                }

                throw new ArgumentException(sb.ToString(), ex);
            }
            catch (ValidationException ex) {
                // For DataAnnotation validations
                var sb = new StringBuilder();
                sb.AppendLine($"Validation failed: {ex.Message}");
                throw new ArgumentException(sb.ToString(), ex);
            }
        }

        protected async virtual Task UpdateDataAsync(TModel model) {
            await _ts.UpdateModelAndCommitAsync<TModel, TEntity>(model);
        }

        public async Task<bool> UpdateModelAsync(TModel model) {
            if (model == null)
                throw new ArgumentNullException($"{nameof(model)} is null!");

            if (!model.DataValidation()) {
                var sb = new StringBuilder();
                if (!string.IsNullOrEmpty(model.Error))
                    sb.AppendLine("Error 1: " + model.Error);
                if (!string.IsNullOrEmpty(model.ErrorMessage))
                    sb.AppendLine("Error 2: " + model.ErrorMessage);
                if (model.ErrorMessages.Length > 0)
                    sb.AppendLine("Error 3: " + model.ErrorMessages.ToString());
                throw new ArgumentException(sb.ToString());
            }

            if (!model.Success)
                throw new ArgumentException(model.Error);

            try {
                await UpdateDataAsync(model);
                return true;
            }
            catch (DbUpdateException ex) {
                var sb = new StringBuilder();
                var innerEx = ex.InnerException;
                int innerLevel = 1;

                while (innerEx != null) {
                    sb.AppendLine($"Inner Exception Level {innerLevel}: {innerEx.Message}\n");

                    if (innerEx is MySqlException mySqlEx)
                        sb.AppendLine($"SQL Error Code: {mySqlEx.Number}\n");

                    innerEx = innerEx.InnerException;
                    innerLevel++;
                }

                if (ex.Entries != null && ex.Entries.Any()) {
                    sb.AppendLine("\nEntities involved in the exception:");
                    foreach (var entry in ex.Entries)
                        sb.AppendLine($"TableName: {entry.Entity.GetType().Name}, Operation: {entry.State}");
                }

                throw new ArgumentException(sb.ToString(), ex);
            }
            catch (ValidationException ex) {
                // For DataAnnotation validations
                var sb = new StringBuilder();
                sb.AppendLine($"Validation failed: {ex.Message}");
                throw new ArgumentException(sb.ToString(), ex);
            }
        }

        protected async virtual Task DeleteDataAsync(Guid id) {
            var tbl = await _ts.GetByIdAsync<TEntity>(id);
            if (tbl == null)
                return;
            await _ts.RemoveAndCommitAsync(tbl);
        }

        public async Task<bool> DeleteByIdAsync(Guid id) {
            if (id.IsNullOrEmpty()) {
                return false;
            }
            try {
                await DeleteDataAsync(id);
                return true;
            }
            catch (Exception ex) {
                throw ex;
            }
        }
        #endregion

        #region Utilities
        public virtual async Task<bool> HasDataAsync(Expression<Func<TEntity, bool>> predicate = null) {
            return await _ts.HasDataAsync(predicate);
        }
        #endregion

        #region Base Cache Methods
        public virtual async Task LoadCachedAsync() {
            var entities = await _ts.GetAllAsync<TEntity>();
            await CacheExtensions.SaveAllToCacheAsync(entities);
        }
        #endregion
    }
}
