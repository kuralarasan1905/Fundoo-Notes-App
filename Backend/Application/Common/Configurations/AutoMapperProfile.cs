using AutoMapper;
using fundoo_notes.Domain.Entities;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Common.Configurations
{
    /// <summary>
    /// Optimized AutoMapper profile with performance configurations
    /// </summary>
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Configure mapping options for better performance
            ConfigureNoteMapping();
            ConfigureLabelMapping();
            ConfigureUserMapping();
            ConfigureTemplateMapping();
            ConfigureReminderMapping();
            ConfigureCollaboratorMapping();
            ConfigureAttachmentMapping();
        }

        private void ConfigureNoteMapping()
        {
            // Note to NoteDto mapping with optimizations
            CreateMap<Note, NoteDto>()
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src =>
                    src.NoteLabels != null ? src.NoteLabels.Select(nl => nl.Label).ToList() : new List<Label>()))
                .ForMember(dest => dest.LabelIds, opt => opt.MapFrom(src =>
                    src.NoteLabels != null ? src.NoteLabels.Select(nl => nl.LabelId).ToList() : new List<int>()))
                .ForMember(dest => dest.Collaborators, opt => opt.MapFrom(src =>
                    src.Collaborators != null ? src.Collaborators.ToList() : new List<Collaborator>()))
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src =>
                    src.Attachments != null ? src.Attachments.ToList() : new List<NoteAttachment>()))
                .ForMember(dest => dest.Reminders, opt => opt.MapFrom(src =>
                    src.Reminders != null ? src.Reminders.ToList() : new List<NoteReminder>()))
                .ForMember(dest => dest.HasReminder, opt => opt.MapFrom(src => src.ReminderDateTime.HasValue))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                    src.User != null ? $"{src.User.FirstName} {src.User.LastName}".Trim() : ""))
                .ForMember(dest => dest.PreviewContent, opt => opt.MapFrom(src =>
                    src.Content.Length > 100 ? src.Content.Substring(0, 100) + "..." : src.Content))
                .AfterMap((src, dest) =>
                {
                    // Post-processing optimizations
                    dest.IsOverdue = src.ReminderDateTime.HasValue && src.ReminderDateTime < DateTime.UtcNow;
                });

            // Note to NoteListDto mapping (optimized for list views)
            CreateMap<Note, NoteListDto>()
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src =>
                    src.NoteLabels != null ? src.NoteLabels.Select(nl => nl.Label).ToList() : new List<Label>()))
                .ForMember(dest => dest.AttachmentCount, opt => opt.MapFrom(src =>
                    src.Attachments != null ? src.Attachments.Count : 0))
                .ForMember(dest => dest.CollaboratorCount, opt => opt.MapFrom(src =>
                    src.Collaborators != null ? src.Collaborators.Count : 0))
                .ForMember(dest => dest.PreviewContent, opt => opt.MapFrom(src =>
                    src.Content.Length > 100 ? src.Content.Substring(0, 100) + "..." : src.Content))
                .ForMember(dest => dest.HasReminder, opt => opt.MapFrom(src => src.ReminderDateTime.HasValue))
                .ForMember(dest => dest.IsListNote, opt => opt.MapFrom(src => false)) // Default to false, can be enhanced later
                .ForMember(dest => dest.CompletedItemsCount, opt => opt.MapFrom(src => 0)) // Default to 0, can be enhanced later
                .ForMember(dest => dest.TotalItemsCount, opt => opt.MapFrom(src => 0)); // Default to 0, can be enhanced later

            // CreateNoteDto to Note mapping
            CreateMap<CreateNoteDto, Note>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.NoteLabels, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Collaborators, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore());

            // UpdateNoteDto to Note mapping
            CreateMap<UpdateNoteDto, Note>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.NoteLabels, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Collaborators, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore());
        }

        private void ConfigureLabelMapping()
        {
            CreateMap<Label, LabelDto>()
                .ForMember(dest => dest.NotesCount, opt => opt.MapFrom(src =>
                    src.NoteLabels != null ? src.NoteLabels.Count(nl => !nl.Note.IsDeleted && !nl.Note.IsTrashed) : 0));

            CreateMap<CreateLabelDto, Label>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.NoteLabels, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            CreateMap<UpdateLabelDto, Label>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.NoteLabels, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());
        }

        private void ConfigureUserMapping()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()));

            CreateMap<RegisterUserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Notes, opt => opt.Ignore())
                .ForMember(dest => dest.Labels, opt => opt.Ignore());
        }

        private void ConfigureTemplateMapping()
        {
            CreateMap<NoteTemplate, NoteTemplateDto>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => 
                    src.User != null ? $"{src.User.FirstName} {src.User.LastName}".Trim() : "System"));

            CreateMap<CreateNoteTemplateDto, NoteTemplate>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());
        }

        private void ConfigureReminderMapping()
        {
            CreateMap<NoteReminder, NoteReminderDto>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Note != null ? src.Note.Title : ""))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Note != null ? src.Note.Content : ""))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Note != null ? src.Note.Color : "#FFFFFF"));

            CreateMap<CreateReminderDto, NoteReminder>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Note, opt => opt.Ignore());
        }

        private void ConfigureCollaboratorMapping()
        {
            CreateMap<Collaborator, CollaboratorDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                    src.User != null ? $"{src.User.FirstName} {src.User.LastName}".Trim() : ""))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src =>
                    src.User != null ? src.User.Email : src.Email ?? ""));

            CreateMap<AddCollaboratorDto, Collaborator>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.UserEmail))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Note, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());
        }

        private void ConfigureAttachmentMapping()
        {
            CreateMap<NoteAttachment, NoteAttachmentDto>();

            CreateMap<CreateNoteAttachmentDto, NoteAttachment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.File.FileName))
                .ForMember(dest => dest.FileType, opt => opt.MapFrom(src => src.File.ContentType))
                .ForMember(dest => dest.FileSize, opt => opt.MapFrom(src => src.File.Length))
                .ForMember(dest => dest.FileUrl, opt => opt.Ignore()) // Will be set by the handler
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Note, opt => opt.Ignore());
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private static bool IsImageFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;
            
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" or ".svg";
        }
    }
}
