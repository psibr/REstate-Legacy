﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REstate.Services.AdminUI
{
    public class FavIconBootstrapper : Psibr.Platform.Nancy.PlatformNancyBootstrapper
    {
        protected override byte[] FavIcon { get; } =
            Convert.FromBase64String(
                @"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");
    }
}
