using System;
using System.Text;
using System.Threading;
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
            if (isImmediate)
            {
                UpdateText(newNumber - _savedTotalNumber);
            }
            else
            {
                _ = CountAddedNumber(newNumber - _savedTotalNumber, cancellationTokenSource);
            }

            _savedTotalNumber = newNumber;
        }

        private void UpdateText(int addedNumber)
        {
            _viewTotalNumber += addedNumber;
            RefreshText();
        }

        private async Awaitable CountAddedNumber(int numberAdded, CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                await CountAddedNumberAsync(numberAdded, cancellationTokenSource);
            }
            catch (OperationCanceledException)
            {
                LogService.Log("Operation CountAddedNumber was cancelled");
            }
            catch (Exception e)
            {
                LogService.LogError(e.Message);
                throw;
            }
        }
        
        private async Awaitable CountAddedNumberAsync(int numberAdded, CancellationTokenSource cancellationTokenSource)
        {
            var numberLeftToAdd = numberAdded;
            var isPositive = numberAdded >= 0;

            while (numberLeftToAdd > 0)
            {
                var numberToAddThisFrame = Mathf.CeilToInt(Time.deltaTime * numberAdded * _textAnimtaionSpeed);

                if ((isPositive && numberToAddThisFrame < numberLeftToAdd) ||
                    (!isPositive && numberToAddThisFrame > numberLeftToAdd))
                {
                    UpdateText(numberToAddThisFrame);
                    numberLeftToAdd -= numberToAddThisFrame;
                }
                else
                {
                    UpdateText(numberLeftToAdd);
                    numberLeftToAdd -= numberLeftToAdd;
                }

                await Awaitable.NextFrameAsync(cancellationToken: cancellationTokenSource.Token);
            }
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