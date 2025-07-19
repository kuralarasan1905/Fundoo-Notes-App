import { Label } from './label';

export interface Note {
  id: string;
  title: string;
  previewContent: string; // Changed from 'description' to 'content' (Google Keep style)
  isPined?: boolean;
  isArchived?: boolean;
  isDeleted?: boolean;
  reminder?: string;
  createdDate?: string;
  modifiedDate?: string;
  color?: string;
  noteLabels?: Label[];
}

// DTO for creating notes - matches your backend CreateNoteDto
export interface CreateNoteDto {
  title: string;
  content: string; // Backend expects 'content' for creation
  color?: string;
  reminderDateTime?: string;
  labelIds?: string[];
}

// DTO for updating notes
export interface UpdateNoteDto {
  title?: string;
  content?: string;
  color?: string;
  reminderDateTime?: string;
  labelIds?: string[];
}
