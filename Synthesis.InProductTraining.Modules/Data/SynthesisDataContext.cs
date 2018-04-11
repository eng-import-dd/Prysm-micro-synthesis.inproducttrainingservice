using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using Synthesis.InProductTrainingService.InternalApi.Models;
using Synthesis.InProductTrainingService.Models;

namespace Synthesis.InProductTrainingService.Data
{
    public class SynthesisDataContext : DbContext
    {
        public SynthesisDataContext() : base("name=SynthesisDatabase")
        {
        }

        public SynthesisDataContext(DbConnection dbConnection) : base(dbConnection, true)
        {
        }

        public virtual DbSet<ClientApplication> ClientApplications { get; set; }
        public virtual DbSet<InProductTrainingSubject> InProductTrainingSubjects { get; set; }
        public virtual DbSet<InProductTrainingView> InProductTrainingViews { get; set; }
        //public virtual DbSet<WizardView> ViewedWizards { get; set; }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException devEx)
            {
                var errorMessages = new StringBuilder(devEx.Message + " The validation errors are: ");

                foreach (var nextDbEntityValidationResult in devEx.EntityValidationErrors)
                {
                    errorMessages.AppendLine($"Entity: {nextDbEntityValidationResult.Entry.Entity.GetType()}; State: {nextDbEntityValidationResult.Entry.State}; Properties:");
                    errorMessages.Append(GetFriendlyValidationMessage(nextDbEntityValidationResult));
                }

                // Throw a new DbEntityValidationException with the improved exception message.
                throw new DbEntityValidationException(errorMessages.ToString(), devEx.EntityValidationErrors, devEx.InnerException);
            }
            catch (DbUpdateConcurrencyException concurrencyEx)
            {
                throw new DbUpdateConcurrencyException(GetDetailedUpdateExceptionMessage(concurrencyEx), concurrencyEx.InnerException);
            }
            catch (DbUpdateException updateEx)
            {
                throw new DbUpdateException(GetDetailedUpdateExceptionMessage(updateEx), updateEx.InnerException);
            }
        }

        private static string GetDetailedUpdateExceptionMessage(DbUpdateException updateEx)
        {
            var errorMessages = new StringBuilder(updateEx.Message + " Entity Details: ");

            foreach (var nextDbEntityEntry in updateEx.Entries)
            {
                var keyProperties = nextDbEntityEntry.Entity.GetType().GetProperties()
                        .Where(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Any()).ToList();

                errorMessages.AppendLine($"Entity: {nextDbEntityEntry.Entity.GetType()}; State: {nextDbEntityEntry.State}");

                foreach (var nextKeyPropertyInfo in keyProperties)
                {
                    var keyValue = nextDbEntityEntry.State == EntityState.Deleted ? nextDbEntityEntry.OriginalValues[nextKeyPropertyInfo.Name] : nextDbEntityEntry.CurrentValues[nextKeyPropertyInfo.Name];
                    errorMessages.AppendLine($"Key Property: {nextKeyPropertyInfo.Name}; Key Value: {keyValue}");
                }

                errorMessages.AppendLine("Properties:");

                foreach (var nextPropertyName in nextDbEntityEntry.CurrentValues.PropertyNames)
                {
                    errorMessages.AppendLine($"Property: {nextPropertyName}; Value: {GetPropertyValue(nextDbEntityEntry, nextPropertyName)}");
                }
            }

            var sqlErrorMessage = GetDetailedExceptionIfInnerSqlExceptionIsSignificant(updateEx);

            if (!string.IsNullOrEmpty(sqlErrorMessage))
            {
                errorMessages.Append(sqlErrorMessage);
            }

            return errorMessages.ToString();
        }

        private static string GetFriendlyValidationMessage(DbEntityValidationResult dbEntityValidationResult)
        {
            var errorMessage = new StringBuilder();
            errorMessage.AppendLine($"Entity: {dbEntityValidationResult.Entry.Entity.GetType()}");

            foreach (var nextDbValidationError in dbEntityValidationResult.ValidationErrors)
            {
                var propertyValue = GetPropertyValue(dbEntityValidationResult.Entry, nextDbValidationError.PropertyName);
                errorMessage.AppendLine($"Validation Error: {nextDbValidationError.ErrorMessage}; Property: {nextDbValidationError.PropertyName}; Value: {propertyValue}");
            }

            return errorMessage.ToString();
        }

        private static object GetPropertyValue(DbEntityEntry dbEntityEntry, string propertyName)
        {
            var upperPropertyName = propertyName.ToUpper();

            /* Do not log any values for any passwords, filename, username fields, etc. */
            if (upperPropertyName.Contains("PASSWORD") ||
                upperPropertyName.Contains("NAME") ||
                upperPropertyName.Contains("EMAIL") ||
                upperPropertyName.Contains("PATH"))
            {
                return "redacted";
            }
            else
            {
                return dbEntityEntry.Member(propertyName).CurrentValue;
            }
        }

        /// <summary>
        /// Checks through all inner exceptions, if any, and throws a new exception if an inner exception 
        /// is of type SqlException and meets additional criteria, otherwise exits. 
        /// Also traces properties of any inner SqlException.
        /// </summary>
        /// <param name="ex">The exception</param>
        private static string GetDetailedExceptionIfInnerSqlExceptionIsSignificant(Exception ex)
        {
            var innerEx = ex.InnerException;
            var sqlExceptionMessage = new StringBuilder();
            while (innerEx != null)
            {
                var sqlEx = innerEx as SqlException;
                if (sqlEx != null)
                {
                    foreach (SqlError sqlError in sqlEx.Errors)
                    {
                        sqlExceptionMessage.AppendLine($"A DbUpdateException has occurred. The inner SqlException details include=>\n[Message: {sqlError.Message}" +
                            $"\nNumber: {sqlError.Number.ToString()}\nState: {sqlError.State.ToString()}\nSource: {sqlError.Source}\nSeverity: {sqlError.Class.ToString()}" +
                            $"\nServer: {sqlError.Server}\nLineNumber: {sqlError.LineNumber.ToString()}\nProcedure: {sqlError.Procedure}]");
                    }
                }

                innerEx = innerEx.InnerException;
            }

            return sqlExceptionMessage.ToString();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClientApplication>()
                .HasMany(e => e.InProductTrainingSubjects)
                .WithRequired(e => e.ClientApplication)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserType>()
                .HasMany(e => e.InProductTrainingViews)
                .WithRequired(e => e.UserType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<InProductTrainingSubject>()
                .HasMany(e => e.InProductTrainingViews)
                .WithRequired(e => e.InProductTrainingSubject)
                .WillCascadeOnDelete(false);

            //modelBuilder.Entity<WizardView>()
            //    .HasRequired(c => c.UserId)
            //    .WithMany()
            //    .HasForeignKey(c => c.UserId)
            //    .WillCascadeOnDelete(true);
        }
    }
}