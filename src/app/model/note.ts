export interface Note {
  id: string;
  title: string;
  description: string;
  isPined?: boolean;
  isArchived?: boolean;
  isDeleted?: boolean;
  reminder?: string;
  createdDate?: string;
  modifiedDate?: string;
  color?: string;
}
