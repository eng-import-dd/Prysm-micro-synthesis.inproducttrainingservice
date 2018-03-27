using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Reflection;
using Synthesis.Cloud.SqlData;

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

        //public virtual DbSet<AccountDomain> AccountDomains { get; set; }
        //public virtual DbSet<AccountImpersonation> AccountImpersonations { get; set; }
        //public virtual DbSet<Account> Accounts { get; set; }
        //public virtual DbSet<AccountSetting> AccountSettings { get; set; }
        //public virtual DbSet<AccountThreshold> AccountThresholds { get; set; }
        //public virtual DbSet<AspectRatio> AspectRatios { get; set; }
        //public virtual DbSet<Banner> Banners { get; set; }
        public virtual DbSet<ClientApplication> ClientApplications { get; set; }
        //public virtual DbSet<FreeEmailDomain> FreeEmailDomains { get; set; }
        //public virtual DbSet<GroupPermission> GroupPermissions { get; set; }
        //public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<InProductTrainingSubject> InProductTrainingSubjects { get; set; }
        public virtual DbSet<InProductTrainingView> InProductTrainingViews { get; set; }
        //public virtual DbSet<ItemConnection> ItemConnections { get; set; }
        //public virtual DbSet<Permission> Permissions { get; set; }
        //public virtual DbSet<PermissionCategory> PermissionCategories { get; set; }
        //public virtual DbSet<PermissionLocation> PermissionLocations { get; set; }
        //public virtual DbSet<Product> Products { get; set; }
        //public virtual DbSet<ProjectFile> ProjectFiles { get; set; }
        //public virtual DbSet<ProjectItem> ProjectItems { get; set; }
        //public virtual DbSet<ProjectParticipant> ProjectParticipants { get; set; }
        //public virtual DbSet<ProjectProjectFile> ProjectProjectFiles { get; set; }
        //public virtual DbSet<Project> Projects { get; set; }
        //public virtual DbSet<RdpProfile> RdpProfiles { get; set; }
        //public virtual DbSet<SchemaVersion> SchemaVersions { get; set; }
        //public virtual DbSet<SnapGrid> SnapGrids { get; set; }
        //public virtual DbSet<SynthesisUser> SynthesisUsers { get; set; }
        //public virtual DbSet<InvitedUsers> InvitedUsers { get; set; }
        //public virtual DbSet<UserAccount> UserAccounts { get; set; }
        //public virtual DbSet<UserGroup> UserGroups { get; set; }
        //public virtual DbSet<UserProject> UserProjects { get; set; }
        //public virtual DbSet<UserRole> UserRoles { get; set; }
        //public virtual DbSet<UserType> UserTypes { get; set; }
        //public virtual DbSet<Workspace> Workspaces { get; set; }
        //public virtual DbSet<LK_Categories> LK_Categories { get; set; }
        //public virtual DbSet<LK_SettingAttributes> LK_SettingAttributes { get; set; }
        //public virtual DbSet<LK_SettingRenderTypes> LK_SettingRenderTypes { get; set; }
        //public virtual DbSet<LK_Settings> LK_Settings { get; set; }
        //public virtual DbSet<Machine> Machines { get; set; }
        //public virtual DbSet<SettingAttributeValue> SettingAttributeValues { get; set; }
        //public virtual DbSet<SettingProfile> SettingProfiles { get; set; }
        //public virtual DbSet<SettingProfileValue> SettingProfileValues { get; set; }
        //public virtual DbSet<CaptureDevice> CaptureDevices { get; set; }
        //public virtual DbSet<Entity> Entities { get; set; }
        //public virtual DbSet<MainMenuItem> MainMenuItems { get; set; }
        //public virtual DbSet<MachineSettingValues2> MachineSettingValues2 { get; set; }
        //public virtual DbSet<ProjectFileFragment> ProjectFileFragments { get; set; }
        //public virtual DbSet<ClientCertificate> ClientCertificate { get; set; }
        //public virtual DbSet<RootCertificate> RootCertificate { get; set; }
        //public virtual DbSet<ProjectFileManifest> ProjectFileManifests { get; set; }
        //public virtual DbSet<QueuedTranscode> QueuedTranscodes { get; set; }
        //public virtual DbSet<LK_ContentProviders> LK_ContentProviders { get; set; }
        //public virtual DbSet<ContentProviderUserConfig> ContentProviderUserConfigs { get; set; }
        //public virtual DbSet<GuestInvite> GuestInvites { get; set; }
        //public virtual DbSet<GuestSession> GuestSessions { get; set; }
        //public virtual DbSet<GuestSessionState> GuestSessionStates { get; set; }
        //public virtual DbSet<ViewedWizard> ViewedWizards { get; set; }
        //public virtual DbSet<BatchExecutionLog> BatchExecutionLog { get; set; }


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
            //modelBuilder.Entity<Account>()
            //   .HasMany(e => e.AccountDomains)
            //   .WithRequired(e => e.Account)
            //   .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Account>()
            //    .HasMany(e => e.AccountImpersonations)
            //    .WithRequired(e => e.Account)
            //    .HasForeignKey(e => e.AccountId)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Account>()
            //    .HasMany(e => e.AccountImpersonations1)
            //    .WithRequired(e => e.Account1)
            //    .HasForeignKey(e => e.AccessedAccountId)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Account>()
            //    .HasMany(e => e.AccountSettings)
            //    .WithRequired(e => e.Account);

            //modelBuilder.Entity<AccountThreshold>()
            //    .Property(e => e.Code)
            //    .IsUnicode(false);

            //modelBuilder.Entity<AccountThreshold>()
            //    .Property(e => e.Description)
            //    .IsUnicode(false);

            //modelBuilder.Entity<AccountThreshold>()
            //    .Property(e => e.Unit)
            //    .IsUnicode(false);

            //modelBuilder.Entity<AccountThreshold>()
            //    .Property(e => e.Logic)
            //    .IsUnicode(false);

            modelBuilder.Entity<ClientApplication>()
                .HasMany(e => e.InProductTrainingSubjects)
                .WithRequired(e => e.ClientApplication)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserType>()
                .HasMany(e => e.InProductTrainingViews)
                .WithRequired(e => e.UserType)
                .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Group>()
            //    .HasMany(e => e.PermissionsForGroup)
            //    .WithRequired(e => e.Group)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Group>()
            //    .HasMany(e => e.UserGroups)
            //    .WithRequired(e => e.Group)
            //    .WillCascadeOnDelete(false);

            modelBuilder.Entity<InProductTrainingSubject>()
                .HasMany(e => e.InProductTrainingViews)
                .WithRequired(e => e.InProductTrainingSubject)
                .WillCascadeOnDelete(false);

            //modelBuilder.Entity<PermissionCategory>()
            //    .HasMany(e => e.Permissions)
            //    .WithRequired(e => e.PermissionCategory)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<PermissionLocation>()
            //    .HasMany(e => e.Permissions)
            //    .WithRequired(e => e.PermissionLocation)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Permission>()
            //    .HasMany(e => e.GroupPermissions)
            //    .WithRequired(e => e.Permission)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Account>()
            //    .HasMany(e => e.AspectRatios)
            //    .WithRequired(e => e.Account)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Account>()
            //    .HasMany(e => e.Groups)
            //    .WithRequired(e => e.Account)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<AspectRatio>()
            //    .HasMany(e => e.SnapGrids)
            //    .WithRequired(e => e.AspectRatio)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<ProjectItem>()
            //    .HasMany(e => e.ItemConnections)
            //    .WithRequired(e => e.ProjectItem)
            //    .HasForeignKey(e => e.ChildProjectItemID)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<ProjectItem>()
            //    .HasMany(e => e.ItemConnections1)
            //    .WithRequired(e => e.ProjectItem1)
            //    .HasForeignKey(e => e.HostProjectItemID)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Project>()
            //    .HasMany(e => e.ProjectFiles)
            //    .WithRequired(e => e.Project)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Project>()
            //    .HasMany(e => e.ProjectItems)
            //    .WithRequired(e => e.Project)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Project>()
            //    .HasMany(e => e.ProjectParticipants)
            //    .WithRequired(e => e.Project)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Project>()
            //    .HasOptional(e => e.ManagerUser)
            //    .WithMany()
            //    .HasForeignKey(x => x.ManagerUserID);

            //modelBuilder.Entity<Project>()
            //    .HasMany(e => e.UserProjects)
            //    .WithRequired(e => e.Project)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Project>()
            //    .HasMany(e => e.Workspaces)
            //    .WithRequired(e => e.Project)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Project>()
            //   .HasMany(e => e.GuestInvites)
            //   .WithRequired(e => e.Project)
            //   .WillCascadeOnDelete(false);

            //modelBuilder.Entity<SnapGrid>()
            //    .Property(e => e.MachineKey)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SynthesisUser>()
            //    .Property(e => e.PasswordHash)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SynthesisUser>()
            //    .Property(e => e.PasswordSalt)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SynthesisUser>()
            //    .HasMany(e => e.ProjectFiles)
            //    .WithOptional(e => e.SynthesisUser)
            //    .HasForeignKey(e => e.AddedByUserId);

            //modelBuilder.Entity<SynthesisUser>()
            //    .HasMany(e => e.UserProjects)
            //    .WithRequired(e => e.SynthesisUser)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<SynthesisUser>()
            //    .HasMany(e => e.InvitesSent)
            //    .WithRequired(e => e.InvitedByUser)
            //    .HasForeignKey(e => e.InvitedBy)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<SynthesisUser>()
            //    .HasMany(e => e.UserGroups)
            //    .WithRequired(e => e.SynthesisUser)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<LK_Categories>()
            //    .HasMany(e => e.LK_Settings)
            //    .WithOptional(e => e.LK_Categories)
            //    .HasForeignKey(e => e.SettingCategoryId);

            //modelBuilder.Entity<LK_SettingRenderTypes>()
            //    .HasMany(e => e.LK_SettingAttributes)
            //    .WithOptional(e => e.LK_SettingRenderTypes)
            //    .HasForeignKey(e => e.AttributeRenderTypeId);

            //modelBuilder.Entity<LK_Settings>()
            //    .HasMany(e => e.LK_SettingAttributes)
            //    .WithRequired(e => e.LK_Settings)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<LK_Settings>()
            //    .HasMany(e => e.LK_Settings1)
            //    .WithOptional(e => e.LK_Settings2)
            //    .HasForeignKey(e => e.ChildSettingId);

            //modelBuilder.Entity<LK_Settings>()
            //    .HasMany(e => e.SettingProfileValues)
            //    .WithRequired(e => e.LK_Settings)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<SettingProfile>()
            //    .HasMany(e => e.SettingProfiles1)
            //    .WithOptional(e => e.SettingProfile1)
            //    .HasForeignKey(e => e.TemplateId);

            //modelBuilder.Entity<SettingProfile>()
            //    .HasMany(e => e.SettingProfileValues)
            //    .WithRequired(e => e.SettingProfile)
            //    .WillCascadeOnDelete(true);

            //modelBuilder.Entity<SettingProfileValue>()
            //    .HasMany(e => e.SettingAttributeValues)
            //    .WithRequired(e => e.SettingProfileValue)
            //    .WillCascadeOnDelete(true);

            //modelBuilder.Entity<SettingProfileValue>()
            //    .HasMany(e => e.SettingProfileValues1)
            //    .WithOptional(e => e.SettingProfileValue1)
            //    .HasForeignKey(e => e.ParentSettingProfileValueId);

            //modelBuilder.Entity<MainMenuItem>()
            //    .HasMany(e => e.Children)
            //    .WithOptional(e => e.ParentMenuItem)
            //    .HasForeignKey(e => e.ParentMenuItemId);

            //modelBuilder.Entity<MainMenuItem>()
            //    .HasRequired(e => e.MainMenuAction);

            //modelBuilder.Entity<Machine>()
            //    .HasMany(e => e.MachineSettingValues2)
            //    .WithRequired(e => e.Machine);

            //modelBuilder.Entity<LK_Settings>()
            //    .HasMany(e => e.MachineSettingValues2)
            //    .WithRequired(e => e.LK_Settings)
            //    .HasForeignKey(e => e.SettingId);

            //modelBuilder.Entity<PermissionLocation>()
            //    .HasMany(e => e.Permissions)
            //    .WithRequired(e => e.PermissionLocation)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<SynthesisUser>()
            //    .HasMany(e => e.UserGroups)
            //    .WithRequired(e => e.SynthesisUser)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<ProjectFile>()
            //    .HasMany(e => e.ProjectFileFragments)
            //    .WithRequired(e => e.ProjectFile)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<ProjectFile>()
            //    .HasMany(p => p.QueuedTranscode)
            //    .WithRequired(q => q.ProjectFile)
            //    .HasForeignKey(q => q.ProjectFileId)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Workspace>()
            //    .HasMany(e => e.ProjectItems)
            //    .WithRequired(e => e.Workspace)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<ClientCertificate>().HasKey(x => x.ClientCertificateId);

            //modelBuilder.Entity<RootCertificate>().HasKey(x => x.RootCertId);

            //modelBuilder.Entity<ProjectFile>()
            //    .HasMany(e => e.ProjectFileManifests)
            //    .WithRequired(e => e.ProjectFile)
            //    .WillCascadeOnDelete(false);
            //modelBuilder.Entity<ContentProviderUserConfig>().HasKey(d => d.ConfigId);

            //modelBuilder.Entity<LK_ContentProviders>()
            //    .HasMany(e => e.ContentProviderUserConfigs)
            //    .WithRequired(e => e.LK_ContentProvider)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<SynthesisUser>()
            //    .HasMany(e => e.ContentProviderUserConfigs)
            //    .WithRequired(e => e.SynthesisUser)
            //    .WillCascadeOnDelete(true);

            //modelBuilder.Entity<LK_ContentProviders>().HasKey(d => d.ProviderId);

            //modelBuilder.Entity<GuestSessionState>()
            //    .HasMany(e => e.GuestSessions)
            //    .WithRequired(e => e.GuestSessionState)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Project>()
            //    .HasMany(e => e.GuestSessions)
            //    .WithRequired(e => e.Project)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<ViewedWizard>()
            //    .HasRequired(c => c.SynthesisUser)
            //    .WithMany()
            //    .HasForeignKey(c => c.UserId)
            //    .WillCascadeOnDelete(true);

            //modelBuilder.Entity<BatchExecutionLog>()
            //    .HasKey(x => x.BatchExecutionId);
        }
    }
}