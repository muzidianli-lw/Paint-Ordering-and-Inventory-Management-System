using System;

namespace PaintStore.API.Application.Common;

public class ErrorCodes
{
    public const string EmailAlreadyExists = "email_already_exists";
    public const string NameAlreadyExists = "name_already_exists";
    public const string VersionConflict = "version_conflict";
    public const string HasRelatedData = "has_related_data";
    public const string UserNotExists = "not_exists";
}
