﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Registration.Dto;
using Notifo.Areas.Api.Controllers.Registration.Dtos;
using Notifo.Domain;
using Notifo.Domain.Channels.WebPush;
using Notifo.Domain.Identity;
using Notifo.Domain.Integrations;
using Notifo.Domain.Subscriptions;
using Notifo.Infrastructure;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers.Registration;

[ApiExplorerSettings(IgnoreApi = true)]
public sealed class RegistrationController(IWebPushService webPushService) : BaseController
{
    [HttpPost("api/web/register")]
    [AppPermission(NotifoRoles.AppWebManager, NotifoRoles.AppUser)]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
    {
        string? userId = null;
        string? userKey = null;

        if (request.CreateUser)
        {
            var userCommand = request.ToUpsert(Guid.NewGuid().ToString());

            var user = await Mediator.SendAsync(userCommand, HttpContext.RequestAborted);

            if (request.Topics?.Length > 0)
            {
                var command = new Subscribe
                {
                    TopicSettings = new ChannelSettings
                    {
                        [Providers.WebPush] = new ChannelSetting
                        {
                            Send = ChannelSend.Send
                        }
                    }
                };

                if (!string.IsNullOrEmpty(request.EmailAddress))
                {
                    command.TopicSettings[Providers.Email] = new ChannelSetting
                    {
                        Send = ChannelSend.Send
                    };
                }

                foreach (var topic in request.Topics.OrEmpty())
                {
                    command.Topic = topic;

                    await Mediator.SendAsync(command, HttpContext.RequestAborted);
                }
            }

            userKey = user!.ApiKey;
            userId = user!.Id;
        }

        var response = new RegisteredUserDto
        {
            PublicKey = webPushService.PublicKey,
            UserId = userId,
            UserToken = userKey
        };

        return Ok(response);
    }
}
