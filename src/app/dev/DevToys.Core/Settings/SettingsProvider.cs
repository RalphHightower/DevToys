﻿using System.Collections;
using System.Globalization;
using DevToys.Api;
using Newtonsoft.Json;

namespace DevToys.Core.Settings;

[Export(typeof(ISettingsProvider))]
internal sealed class SettingsProvider : ISettingsProvider
{
    private readonly ISettingsStorage _settingsStorage;

    [ImportingConstructor]
    public SettingsProvider(ISettingsStorage settingsStorage)
    {
        _settingsStorage = settingsStorage;
    }

    public event EventHandler<SettingChangedEventArgs>? SettingChanged;

    public T GetSetting<T>(SettingDefinition<T> settingDefinition)
    {
        if (_settingsStorage.TryReadSetting(settingDefinition.Name, out object? settingValue))
        {
            if (typeof(T).IsEnum)
            {
                return (T)Enum.Parse(typeof(T), settingValue?.ToString() ?? string.Empty);
            }
            else if (typeof(IList).IsAssignableFrom(typeof(T)))
            {
                return JsonConvert.DeserializeObject<T>(settingValue?.ToString() ?? string.Empty)!;
            }

            object? result = Convert.ChangeType(settingValue, typeof(T), CultureInfo.InvariantCulture);
            if (result is T strongTypedResult)
            {
                return strongTypedResult;
            }
        }

        SetSetting(settingDefinition, settingDefinition.DefaultValue);
        return settingDefinition.DefaultValue;
    }

    public void SetSetting<T>(SettingDefinition<T> settingDefinition, T value)
    {
        object? valueToSave = value;
        if (value is Enum valueEnum)
        {
            valueToSave = valueEnum.ToString();
        }
        else if (value is IList list)
        {
            valueToSave = JsonConvert.SerializeObject(list, Formatting.None);
        }

        _settingsStorage.WriteSetting(settingDefinition.Name, valueToSave);

        SettingChanged?.Invoke(this, new SettingChangedEventArgs(settingDefinition.Name, value));
    }

    public void ResetSetting<T>(SettingDefinition<T> settingDefinition)
    {
        _settingsStorage.ResetSetting(settingDefinition.Name);

        SettingChanged?.Invoke(this, new SettingChangedEventArgs(settingDefinition.Name, settingDefinition.DefaultValue));
    }
}