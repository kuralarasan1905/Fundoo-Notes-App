export interface Label {
  id: string;
  name: string;  // Backend uses 'Name' property
  label?: string; // Keep for backward compatibility, will be mapped from 'name'
  color?: string;
  isDeleted?: boolean;
  userId?: string;
}
