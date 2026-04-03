using System;
using System.Text;
using System.Threading;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.Logger.Base;
using TMPro;
using UnityEngine;

namespace CoreDomain.Scripts.Helpers
{
    public class CountableTextView : MonoBehaviour
    {
        private static readonly int UNDERLAY_COLOR_SHADER_PROPERTY = Shader.PropertyToID("_UnderlayColor");
        private const char ZERO_DIGIT = '0';

        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private float _textAnimtaionSpeed = 2;
        [SerializeField] private int _minNumOfDigits = 6;
        [SerializeField] private string _prefixText = "";
        [SerializeField] private Color _normalColor;
        [SerializeField] private Color _passedGoalColor;
        
        private int _savedTotalNumber;
        private int _viewTotalNumber;
        private string _zeroDigits = "";
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private int _goalNumber;
        
        private CancellationTokenSource _currentCountingCancellationTokenSource;

        public void SetNumber(int number)
        {
            _viewTotalNumber = number;
            _savedTotalNumber = number;
            RefreshText();
        }
        
        public void SetGoalNumber(int number)
        {
            _goalNumber = number;
            RefreshText();
        }

        public void CountToNumber(int newNumber, CancellationTokenSource cancellationTokenSource, bool isImmediate = false)
        {
            CountToNumberAsync(newNumber, cancellationTokenSource, isImmediate).Forget();
        }
        
        private async Awaitable CountToNumberAsync(int newNumber, CancellationTokenSource cancellationTokenSource, bool isImmediate = false)
        {
            _currentCountingCancellationTokenSource?.Cancel();
            _currentCountingCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
            
            if (isImmediate)
            {
                UpdateText(newNumber);
            }
            else
            {
                try
                {
                    await CountToNumberInternal(newNumber, _currentCountingCancellationTokenSource);
                }
                finally
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        UpdateText(newNumber);
                    }
                }
            }

            _savedTotalNumber = newNumber;
        }

        private void UpdateText(int newNumber)
        {
            _viewTotalNumber = newNumber;
            RefreshText();
        }
        
        private async Awaitable CountToNumberInternal(int targetNumber, CancellationTokenSource cancellationTokenSource)
        {
            var totalDistance = Mathf.Abs(targetNumber - _viewTotalNumber);
            var distanceTravelled = 0;

            while (distanceTravelled < totalDistance)
            {
                var rawDelta = targetNumber - _viewTotalNumber;
                var step = rawDelta * Time.deltaTime * _textAnimtaionSpeed;
                var moveAmount = (rawDelta > 0) ? Mathf.Max(1, Mathf.RoundToInt(step)) 
                    : Mathf.Min(-1, Mathf.RoundToInt(step));

                var willOverStepTargetThisFrame = Mathf.Abs(moveAmount) >= Mathf.Abs(targetNumber - _viewTotalNumber);
                if (willOverStepTargetThisFrame)
                {
                    break; 
                }

                _viewTotalNumber += moveAmount;
                RefreshText();
                distanceTravelled += Mathf.Abs(moveAmount);
                await Awaitable.NextFrameAsync(cancellationTokenSource.Token);
            }

            _viewTotalNumber = targetNumber;
            RefreshText();
        }

        private void RefreshText()
        {
            var stringTotalNumber = _viewTotalNumber.ToString();
            var numOfZeros = _minNumOfDigits - stringTotalNumber.Length;

            if (numOfZeros != _zeroDigits.Length)
            {
                for (var i = 0; i < numOfZeros; i++)
                {
                    _stringBuilder.Append(ZERO_DIGIT);
                }

                _zeroDigits = _stringBuilder.ToString();
                _stringBuilder.Clear();
            }

            var goalText = "";
            bool doesSupportGoal = _goalNumber > 0;

            if (doesSupportGoal)
            {
                goalText = "/" + _goalNumber;
                UpdateTextColorAccordingToGoal();
            }

            _text.text = _prefixText + _zeroDigits + stringTotalNumber + goalText;
        }

        private void UpdateTextColorAccordingToGoal()
        {
            if (_viewTotalNumber < _goalNumber)
            {
                _text.fontMaterial.SetColor(UNDERLAY_COLOR_SHADER_PROPERTY, _normalColor);
            }
            else
            {
                _text.fontMaterial.SetColor(UNDERLAY_COLOR_SHADER_PROPERTY, _passedGoalColor);
            }
        }
    }
}