using System;

namespace PaintStore.API.Application.Common;

public class ErrorCodes
{
    public const string EmailAlreadyExists = "email_already_exists";
    public const string NameAlreadyExists = "name_already_exists";
    public const string VersionConflict = "version_conflict";
    public const string HasRelatedData = "has_related_data";
    public const string UserNotExists = "user_not_exists";
    public const string PaintProductNotExist = "paint_product_not_exists";
    public const string OrderNotExist = "order_not_exists";
    public const string PaintProductInentoryNotEnough = "paint_product_inventory_not_enough";
}
