using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using mt5_term_api;


namespace MetaRPC.CSharpMT5
{
    public static class Constants
    {
        public const string DefaultSymbol = "EURUSD";
        public const double DefaultVolume = 0.1;
        public const string DefaultServer = "MetaQuotes-Demo";
    }
}
