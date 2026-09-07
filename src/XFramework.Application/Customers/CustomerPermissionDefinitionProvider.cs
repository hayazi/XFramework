using XFramework.Application.Contracts.Authorization;

namespace XFramework.Application.Customers;

public sealed class CustomerPermissionDefinitionProvider
    : IPermissionDefinitionProvider
{
    public void Define(
        IPermissionDefinitionContext context)
    {
        var crm =
            context.AddGroup(
                "CRM",
                "Customer Relationship Management");

        var customer =
            context.AddPermission(
                "CRM.Customer",
                "Customers",
                crm);

        context.AddPermission(
            "CRM.Customer.View",
            "View",
            customer);

        context.AddPermission(
            "CRM.Customer.Create",
            "Create",
            customer);

        context.AddPermission(
            "CRM.Customer.Edit",
            "Edit",
            customer);

        context.AddPermission(
            "CRM.Customer.Delete",
            "Delete",
            customer);
    }
}