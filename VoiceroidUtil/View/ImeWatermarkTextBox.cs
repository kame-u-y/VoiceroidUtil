﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace VoiceroidUtil.View
{
    /// <summary>
    /// IMEの挙動を考慮した即時反映バインディングを行う、
    /// <see cref="WatermarkTextBox"/> の派生クラス。
    /// </summary>
    /// <remarks>
    /// <para>
    /// Windows10 May 2020 Update 以降の新MS-IMEが、 <see cref="Text"/> プロパティに
    /// UpdateSourceTrigger=PropertyChanged でバインディングされた
    /// TextBox での変換時におかしな挙動となるため、その対処のために用意したクラス。
    /// </para>
    /// <para>
    /// <see cref="Text"/> プロパティへのバインディング時は
    /// UpdateSourceTrigger=Explicit とすることを推奨する。
    /// </para>
    /// </remarks>
    public class ImeWatermarkTextBox : WatermarkTextBox
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public ImeWatermarkTextBox()
        {
            this.SelectionChanged += this.OnSelectionChanged;
        }


        private static void OnBindableSelectionStartChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs args)
        {
            var textBox = dependencyObject as ImeWatermarkTextBox;

            if (!textBox.changeFromUI)
            {
                textBox.SelectionStart = (int)args.NewValue;
            }
            else
            {
                textBox.changeFromUI = false;
            }
        }

        public static readonly DependencyProperty BindableSelectionStartProperty =
            DependencyProperty.Register(
                "BindableSelectionStart",
                typeof(int),
                typeof(ImeWatermarkTextBox),
                new PropertyMetadata(OnBindableSelectionStartChanged));

        public int BindableSelectionStart
        {
            get => (int)this.GetValue(BindableSelectionStartProperty);
            set => this.SetValue(BindableSelectionStartProperty, value);
        }

        private bool changeFromUI;

        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (this.BindableSelectionStart != this.SelectionStart)
            {
                this.changeFromUI = true;
                this.BindableSelectionStart = this.SelectionStart;
            }

        }

        /// <summary>
        /// <see cref="Text"/> プロパティの変更時に呼び出される。
        /// </summary>
        /// <param name="e">イベント引数。</param>
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            // 自前でバインディングソース更新
            this.GetBindingExpression(TextProperty).UpdateSource();

            base.OnTextChanged(e);
        }
    }
}
