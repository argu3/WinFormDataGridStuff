using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static AssetStatusInfo.InputConfiguration;

namespace AssetStatusInfo
{
    internal class InputConfigurations
    {
        private Dictionary<string, InputConfiguration> _configurations;
        public Dictionary<string, InputConfiguration> configurations
        {  get => _configurations; }
        public InputConfigurations()
        {
            _configurations = new Dictionary<string, InputConfiguration>();
        }

        public void AddConfiguration(string name, InputConfiguration config)
        {
            _configurations.Add(name, config);
        }
        public bool Validate(DataGridViewCell cell)
        {
            if(configurations.ContainsKey(cell.OwningColumn.Name))
            {
                if (cell.Value != null)
                {
                    return configurations[cell.OwningColumn.Name].Validate(cell.Value.ToString());
                }
            }
            return false;
        }
    }

    internal class InputConfigurationBuilder : InputConfiguration
    {
        //private InputConfiguration config;
        private List<string> _codeWords = new List<string>();
        private KeyValuePair<uint?, bool> _maximumLength = new KeyValuePair<uint?, bool>(null, false);
        private KeyValuePair<uint?, bool> _minimumLength = new KeyValuePair<uint?, bool>(null, false);
        private KeyValuePair<uint?, bool> _length = new KeyValuePair<uint?, bool>(null, false);
        private KeyValuePair<int?, bool> _minimumValue = new KeyValuePair<int?, bool>(null, false);
        private KeyValuePair<int?, bool> _maximumValue = new KeyValuePair<int?, bool>(null, false);
        private Type _typeOf;
        private bool showMessage = true;
        private Predicate<String>? _comparer;

        public InputConfigurationBuilder(Type TypeOf) : base(TypeOf)
        {
            _typeOf = TypeOf;
        }
        public void Reset(Type TypeOf)
        {
            _typeOf = TypeOf;
            _codeWords = new List<string>();
            _maximumLength = new KeyValuePair<uint?, bool>(null, false);
            _minimumLength = new KeyValuePair<uint?, bool>(null, false);
            _length = new KeyValuePair<uint?, bool>(null, false);
            _minimumValue = new KeyValuePair<int?, bool>(null, false);
            _maximumValue = new KeyValuePair<int?, bool>(null, false);
            showMessage = true;
            _comparer = null; ;
        }
        public void ChangeType(Type TypeOf)
        {
            _typeOf = TypeOf;
        }
        public void HasLength(uint length)
        {
            if(_maximumLength.Value || _minimumLength.Value)
            {
                throw new Exception("Range of lengths already set");
            }
            else if(length == 0)
            {
                throw new Exception("Length can't be 0");
            }
            _length = new KeyValuePair<uint?, bool>(length,true);
        }
        public void HasLengthMinMax(uint min = 0, uint max = 0)
        {
            if(_length.Value)
            {
                throw new Exception("Static of length already set");
            }
            else if (min >= max && max != 0)
            {
                throw new ArgumentException("minimum length needs to be less than the maximum length");
            }
            if (min > 0) { _minimumLength = new KeyValuePair<uint?, bool>(min, true); };
            if (max > 0) { _maximumLength = new KeyValuePair<uint?, bool>(max, true); };
        }
        public void HasValueMinMax(int? min, int? max)
        {
            if(min != null && max != null ? true : min >= max)
            {
                throw new ArgumentException("minimum value needs to be less than the maximum value");
            }
            else
            {
                if (min != null){_minimumValue = new KeyValuePair<int?, bool>(min, true);};
                if (max != null){_maximumValue = new KeyValuePair<int?, bool>(max, true);};
            }
        }

        public void HasCodeWords(List<string> codeWords)
        {
            _codeWords = codeWords;
        }
        public void WillShowMessage(bool show)
        {
            showMessage = show;
        }
        public void HasComparisonFunction(Predicate<string> comparer)
        {
            _comparer = comparer;
        }
        public InputConfiguration Build()
        {
            return CreateInputConfig(_comparer, _typeOf, _minimumValue, _maximumValue, _length, _minimumLength, _maximumLength, _codeWords, showMessage);
             
        }
    }
    public class InputConfiguration
    {
        protected List<string> _codeWords = new List<string>();
        protected KeyValuePair<uint?, bool> _maximumLength = new KeyValuePair<uint?, bool>(null, false);
        protected KeyValuePair<uint?, bool> _minimumLength = new KeyValuePair<uint?, bool>(null, false);
        protected KeyValuePair<uint?,bool> _length = new KeyValuePair<uint?, bool>(null, false);
        protected KeyValuePair<int?, bool> _minimumValue = new KeyValuePair<int?, bool>(null, false);
        protected KeyValuePair<int?, bool> _maximumValue = new KeyValuePair<int?, bool>(null, false);
        protected Type _typeOf;
        protected bool showMessage = true;
        protected Predicate<String>? _comparer;
        //need to implement
        private Regex? _regex;
        public InputConfiguration(Type TypeOf)
        {
            _typeOf = TypeOf;
        }
        protected static InputConfiguration CreateInputConfig(Predicate<String>? Comparer, Type TypeOf, KeyValuePair<int?, bool> MinimumValue, KeyValuePair<int?, bool> MaximumValue, KeyValuePair<uint?, bool> Length, KeyValuePair<uint?, bool> MaximumLength, KeyValuePair<uint?, bool> MinimumLength, List<string>? CodeWords, bool ShowMessage)
        {
            return new InputConfiguration(Comparer, TypeOf, MinimumValue, MaximumValue, Length, MaximumLength, MinimumLength, CodeWords, ShowMessage);
        }
        protected InputConfiguration(Predicate<String>? Comparer, Type TypeOf, KeyValuePair<int?, bool> MinimumValue, KeyValuePair<int?, bool> MaximumValue, KeyValuePair<uint?, bool> Length, KeyValuePair<uint?, bool> MinimumLength, KeyValuePair<uint?, bool> MaximumLength, List<string>? CodeWords, bool ShowMessage)
        {
            _comparer = Comparer;
            _typeOf = TypeOf;
            _maximumLength = MaximumLength;
            _minimumLength = MinimumLength;
            _maximumValue = MaximumValue;
            _maximumValue = MaximumValue;
            _length = Length;
            _codeWords = CodeWords;
            showMessage = ShowMessage;
        }
        public uint? length
        {
            get => _length.Value ? _length.Key : null;
        }
        public Type typeOf
            { get => _typeOf; }
        public int? minimumValue
        {
            get => _minimumValue.Value ? _minimumValue.Key : null;
        }
        public int? maximumValue
        {
            get => _maximumValue.Value ? _maximumValue.Key : null;
        }
        public uint? maximumLength
        {
            get => _maximumLength.Value ? _maximumLength.Key : null;
        }
        public uint? minimumLength
        {
            get => _minimumLength.Value ? _minimumLength.Key : null;
        }
        public List<string> codeWords
        {
            get => _codeWords;

        }
        public bool Validate(string obj)
        {
            if (obj == "")
            {
                return false;
            }
            else
            { 
                var validated = _Validate(obj);
                if(!validated.Item1 && showMessage) 
                {
                    MessageBox.Show(validated.Item2, "Incorrect Entry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return validated.Item1;
            }
        }
        private (bool, string) _Validate(string obj)
        {
            //check if it matches a pre-set option
            if (_codeWords.Count > 0 && _codeWords.Contains(obj))
            {
                return (true, "");
            }
            //check type
            try
            {
                var test = Convert.ChangeType(obj, _typeOf);
            }
            catch (Exception)
            {
                if(IsNumericType(_typeOf))
                {
                    return (false, $"This is the wrong type. It needs to be a number of type '{_typeOf.Name}'");
                }
                return (false, $"This is the wrong type. It needs to be a {_typeOf.Name}");
            }
            //check range
            if (IsNumericType(_typeOf))
            {
                decimal value = Convert.ToDecimal(obj);
                if(value > _maximumValue.Key && _maximumValue.Value)
                {
                    return (false, $"Maximum value is {_maximumValue.Key}");
                }
                if(value < _minimumValue.Key && _minimumValue.Value)
                {
                    return (false, $"Maximum value is {_minimumValue.Key}");
                }
            }
            //check length
            if(_length.Value && obj.Length != _length.Key)
            {
                return (false, $"Must have {_length.Key} characters");
            }
            if(_maximumLength.Value && obj.Length > _maximumLength.Key)
            {
                return (false, $"Must be less than {_maximumLength.Key} characters");
            }
            else if (_minimumLength.Value && obj.Length < _minimumLength.Key)
            {
                return (false, $"Must be greater than {_minimumLength.Key} characters");
            }
            //check arbitrary function to validate
            if (_comparer == null ? false : !_comparer(obj))
            {
                return (false, "failed validation"); 
            }
            return (true, "");
        }
        private static bool IsNumericType(Type t)
        //https://stackoverflow.com/questions/1749966/c-sharp-how-to-determine-whether-a-type-is-a-number
        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }

}
