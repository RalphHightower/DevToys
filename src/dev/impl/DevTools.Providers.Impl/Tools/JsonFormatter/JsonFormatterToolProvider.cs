﻿#nullable enable

using DevTools.Common;
using DevTools.Core.Injection;
using DevTools.Core.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Composition;

namespace DevTools.Providers.Impl.Tools.JsonFormatter
{
    [Export(typeof(IToolProvider))]
    [Name("Json Formatter")]
    [ProtocolName("jsonformat")]
    [Order(0)]
    [NotScrollable]
    internal sealed class JsonFormatterToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string DisplayName => LanguageManager.Instance.JsonFormatter.DisplayName;

        public object IconSource => CreatePathIconFromPath(nameof(JsonFormatterToolProvider));

        [ImportingConstructor]
        public JsonFormatterToolProvider(IThread thread, IMefProvider mefProvider)
            : base(thread)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return IsValidJson(data);
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<JsonFormatterToolViewModel>();
        }

        private static bool IsValidJson(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            input = input.Trim();

            if ((input.StartsWith("{") && input.EndsWith("}")) //For object
                || (input.StartsWith("[") && input.EndsWith("]"))) //For array
            {
                try
                {
                    JToken? jtoken = JToken.Parse(input);
                    return jtoken is not null;
                }
                catch (JsonReaderException)
                {
                    // Exception in parsing json. It likely mean the text isn't a JSON.
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    // TODO: Log this.
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}