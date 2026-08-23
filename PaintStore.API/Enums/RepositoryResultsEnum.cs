namespace PaintStore.API.Enums;

public enum RepositoryResultsEnum
{
    Success,
    ConcurrencyException,
    UniqueIndexDuplicated,
    ForeignKeyConstraintViolation
}
