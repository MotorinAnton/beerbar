using System;
using System.Collections.Generic;
using System.Linq;
using Core.Services;

namespace Core.Save
{
    [Serializable]
    public class SaveParameters
    {
        public List<SaveParameter> SaveParametersList = new List<SaveParameter>();

        private Dictionary<string, ValueParameter> _parametersDictionary;

        public void Initialize()
        {
            _parametersDictionary = new Dictionary<string, ValueParameter>();
            _parametersDictionary.Clear();

            var saveParametersService = GameServicesUtilities.Get<SaveParametersService>();
            
            foreach (var parameter in saveParametersService.Parameters
                         .Where(parameter => SaveParametersList
                             .All(x => x.Key != parameter.Key.Name)))
            {
                SaveParametersList.Add(new SaveParameter
                {
                    Key = parameter.Key.Name,
                    Value = parameter.Value.GetDefaultStringValue()
                });
            }

            foreach (var saveParameter in SaveParametersList)
            {
                var parameterType = saveParametersService.Parameters
                    .First(x => x.Key.Name == saveParameter.Key).Value.GetParameterType();

                _parametersDictionary.Add(saveParameter.Key,
                    GetTypedValueParameter(parameterType, saveParameter.Value));
            }
        }

        public void SetValue<T>(string key, T value) => ((ValueParameter<T>)_parametersDictionary[key]).Value = value;

        public T GetValue<T>(string key) => ((ValueParameter<T>)_parametersDictionary[key]).Value;

        public void BeforeSave()
        {
            SaveParametersList.Clear();

            foreach (var parameter in _parametersDictionary)
            {
                SaveParametersList.Add(new SaveParameter
                {
                    Key = parameter.Key,
                    Value = parameter.Value.StringValue
                });
            }
        }

        private ValueParameter GetTypedValueParameter(ParameterType parameterType, string stringValue)
        {
            switch (parameterType)
            {
                case ParameterType.Int:
                    return new ValueParameter<int>()
                    {
                        Value = int.Parse(stringValue)
                    };
                case ParameterType.Float:
                    return new ValueParameter<float>()
                    {
                        Value = float.Parse(stringValue)
                    };
                case ParameterType.String:
                default:
                    return new ValueParameter<string>()
                    {
                        Value = stringValue
                    };
            }
        }
    }

    [Serializable]
    public class SaveParameter
    {
        public string Key;
        public string Value;
    }

    public abstract class ValueParameter
    {
        public abstract string StringValue { get; }
    }

    public class ValueParameter<T> : ValueParameter
    {
        public T Value;

        public override string StringValue => Value.ToString();
    }
}