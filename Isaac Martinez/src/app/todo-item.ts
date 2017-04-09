export class TodoItem {
    id: number;
    title: string;
    description: string;
    createdOnUtc: Date;
    updatedOnUtc?: Date;
    deletedOnUtc?: Date;
}
