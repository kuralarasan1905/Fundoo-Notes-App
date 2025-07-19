using AutoMapper;
using fundoo_notes.Domain.Entities;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Application.Features.Templates.Commands.CreateTemplate;

namespace fundoo_notes.Application.Common.Mappings
{
    /// <summary>
    /// AutoMapper profile for entity to DTO mappings
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));

            CreateMap<User, UserProfileDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.TotalNotes, opt => opt.Ignore())
                .ForMember(dest => dest.TotalLabels, opt => opt.Ignore());

            CreateMap<UserRegistrationDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

            CreateMap<UpdateUserProfileDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

            // Note mappings
            CreateMap<Note, NoteDto>()
                .ForMember(dest => dest.PreviewContent, opt => opt.MapFrom(src => src.PreviewContent))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src => src.NoteLabels.Select(nl => nl.Label)))
                .ForMember(dest => dest.Collaborators, opt => opt.MapFrom(src => src.Collaborators));

            CreateMap<Note, NoteListDto>()
                .ForMember(dest => dest.PreviewContent, opt => opt.MapFrom(src => src.PreviewContent))
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src => src.NoteLabels.Select(nl => nl.Label)))
                .ForMember(dest => dest.CollaboratorCount, opt => opt.MapFrom(src => src.Collaborators.Count))
                .ForMember(dest => dest.AttachmentCount, opt => opt.MapFrom(src => src.Attachments.Count))
                .ForMember(dest => dest.CompletedItemsCount, opt => opt.MapFrom(src => src.CompletedItemsCount))
                .ForMember(dest => dest.TotalItemsCount, opt => opt.MapFrom(src => src.TotalItemsCount));

            CreateMap<CreateNoteDto, Note>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.NoteLabels, opt => opt.Ignore())
                .ForMember(dest => dest.Collaborators, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

            CreateMap<UpdateNoteDto, Note>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.NoteLabels, opt => opt.Ignore())
                .ForMember(dest => dest.Collaborators, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

            // Label mappings
            CreateMap<Label, LabelDto>()
                .ForMember(dest => dest.NotesCount, opt => opt.MapFrom(src => src.NoteLabels.Count));

            CreateMap<CreateLabelDto, Label>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.NoteLabels, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

            CreateMap<UpdateLabelDto, Label>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.NoteLabels, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

            // Collaborator mappings
            CreateMap<Collaborator, CollaboratorDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));

            // Attachment mappings
            CreateMap<NoteAttachment, NoteAttachmentDto>();

            // Template mappings
            CreateMap<NoteTemplate, NoteTemplateDto>();
            CreateMap<CreateTemplateCommand, NoteTemplate>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.IsPublic, opt => opt.Ignore())
                .ForMember(dest => dest.UsageCount, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

            // History mappings
            CreateMap<NoteHistory, NoteHistoryDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName));
        }
    }
}
