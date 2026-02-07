// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

global using MediatR;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.RazorPages;
global using Microsoft.Extensions.Localization;
global using Microsoft.AspNetCore.Authorization;
global using Domain.Common;
global using Domain.Entities;
global using Domain.Enums;
global using Domain.Exceptions;
global using Application.Common.Interfaces.Identity;
global using Application.Common.Interfaces.Identity.DTOs;
global using Application.Common.Models;
global using Application.Settings;
global using Infrastructure.Constants.ClaimTypes;
global using Infrastructure.Extensions;
global using Infrastructure.Identity;
