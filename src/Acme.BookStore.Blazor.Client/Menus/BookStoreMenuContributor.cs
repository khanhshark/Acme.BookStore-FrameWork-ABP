﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Acme.BookStore.Localization;
using Acme.BookStore.MultiTenancy;
using Volo.Abp.Account.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity.Blazor;
using Volo.Abp.SettingManagement.Blazor.Menus;
using Volo.Abp.TenantManagement.Blazor.Navigation;
using Volo.Abp.UI.Navigation;
using Acme.BookStore.Permissions;

namespace Acme.BookStore.Blazor.Client.Menus;

public class BookStoreMenuContributor : IMenuContributor
{
    private readonly IConfiguration _configuration;

    public BookStoreMenuContributor(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
        else if (context.Menu.Name == StandardMenus.User)
        {
            await ConfigureUserMenuAsync(context);
        }
    }

    private async Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<BookStoreResource>();

        // Thêm menu "Home" vào vị trí đầu tiên
        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                "BookStore.Home",
                l["Menu:Home"],
                "/",
                icon: "fas fa-home"
            )
        );

        // Thêm menu sách (BooksStore)
        var bookStoreMenu = new ApplicationMenuItem(
            "BooksStore",
            l["Menu:BookStore"],
            icon: "fa fa-book"
        );
        context.Menu.AddItem(bookStoreMenu);

        // Kiểm tra quyền trước khi thêm mục "Books"
        if (await context.IsGrantedAsync(BookStorePermissions.Books.Default))
        {
            bookStoreMenu.AddItem(new ApplicationMenuItem(
                "BooksStore.Books",
                l["Menu:Books"],
                url: "/books"
            ));
        }
        if (await context.IsGrantedAsync(BookStorePermissions.Authors.Default))
        {
            context.Menu.AddItem(new ApplicationMenuItem(
                "BooksStore.Authors",
                l["Menu:Authors"],
                url: "/authors"
            ));
        }

        // Lấy menu "Administration"
        var administration = context.Menu.GetAdministration();

        // Kiểm tra Multi-Tenancy và cấu hình menu tenant
        if (MultiTenancyConsts.IsEnabled)
        {
            administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
        }
        else
        {
            administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
        }

        // Sắp xếp thứ tự các mục con trong menu "Administration"
        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
        administration.SetSubItemOrder(SettingManagementMenus.GroupName, 3);
        
    }


    private Task ConfigureUserMenuAsync(MenuConfigurationContext context)
    {
        var accountStringLocalizer = context.GetLocalizer<AccountResource>();

        var authServerUrl = _configuration["AuthServer:Authority"] ?? "";

        context.Menu.AddItem(new ApplicationMenuItem(
            "Account.Manage",
            accountStringLocalizer["MyAccount"],
            $"{authServerUrl.EnsureEndsWith('/')}Account/Manage",
            icon: "fa fa-cog",
            order: 1000,
            target: "_blank").RequireAuthenticated());

        return Task.CompletedTask;
    }
}
