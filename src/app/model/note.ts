export interface Note {
  id: string;
  title: string;
  description: string;
  isPined?: boolean;
  isArchived?: boolean;
  isDeleted?: boolean;
  reminder?: any[];
  createdDate?: string;
  modifiedDate?: string;
  color?: string;
  label?: any[];
  imageUrl?: string;
  linkUrl?: string;
  userId?: string;
  noteCheckLists?: any[];
  noteLabels?: any[];
}
