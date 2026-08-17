using System;

namespace PaintStore.API.Application.Common;

public class UserErrorCodes
{
    public const string EmailAlreadyExists = "user.email_already_exists";
    public const string VersionConflict = "user.version_conflict";
    public const string HasRelatedData = "user.has_related_data";
    public const string UserNotExists = "user.user_not_exists";
}
