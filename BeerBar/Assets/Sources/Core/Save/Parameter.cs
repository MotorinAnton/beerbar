using System;
using Core.Services;
using UnityEngine;

namespace Core.Save
{
    public interface IParameter
    {
        public ParameterType GetParameterType();
        public string GetDefaultStringValue();
    }

    public enum ParameterType
    {
        Int,
        Float,
        String
    }

    public abstract class Parameter<T> : IParameter
    {
        public abstract ParameterType GetParameterType();

        public string GetDefaultStringValue() => DefaultValue.ToString();

        protected string Id => GetType().Name;

        protected virtual T DefaultValue => default;

        public void Apply(T value)
        {
            Set(value);
            OnApply(Get());
        }

        public abstract T Get();

        protected abstract void Set(T value);

        protected abstract void OnApply(T current);
    }

    public abstract class FloatParameter : Parameter<float>
    {
        public override ParameterType GetParameterType() => ParameterType.Float;

        public override float Get()
        {
            return GameServicesUtilities.Get<SaveService>().GetParameter<float>(Id);
        }

        protected override void Set(float value)
        {
            GameServicesUtilities.Get<SaveService>().SetParameter(Id, value);
        }
    }

    [Serializable]
    public class MasterVolumeParameter : FloatParameter
    {
        protected override float DefaultValue => 1f;

        protected override void OnApply(float current)
        {
            Debug.Log($"New master volume value: {current}");
        }
    }
}