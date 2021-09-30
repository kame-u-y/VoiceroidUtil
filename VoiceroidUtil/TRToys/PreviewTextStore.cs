using RucheHome.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VoiceroidUtil.TRToys
{
    /// <summary>
    /// TRT's拡張：プレビューのItemsControl用のストア
    /// </summary>
    public class PreviewTextStore : BindableBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="text">プレビューテキストの一行</param>
        /// <param name="style">プレビューの設定値</param>
        public PreviewTextStore(string text, PreviewStyle style)
        {
            text = text.Replace(Environment.NewLine, ";");
            this.Text = text;
            this.Indices = GetIndices(text, style);
        }

        /// <summary>
        /// プレビューテキストの一行。GlyphsのUnicodeStringへバインドする。
        /// </summary>
        public string Text
        {
            get => this.text;
            set => SetProperty(ref text, value);
        }
        private string text;

        /// <summary>
        /// GlyphsのIndicesを用いて字間を調整する。
        /// </summary>
        public string Indices
        {
            get => this.indices;
            set => SetProperty(ref indices, value);
        }
        private string indices;

        /// <summary>
        /// フォントに入力文字の情報が存在しなかった場合の大体文字。
        /// ほぼ必ず存在し、かつ適度な幅の文字としてAを使用
        /// </summary>
        //readonly char TempLetter = 'A';

        /// <summary>
        /// テキストと設定値からIndicesを生成。
        /// </summary>
        /// <param name="text">プレビューテキストの一行</param>
        /// <param name="style">プレビューの設定値</param>
        /// <returns></returns>
        private string GetIndices(string text, PreviewStyle style)
        {
            var typeface = new GlyphTypeface(new Uri(style.Text.PreviewFontUriString));
            var indices = "";
            for (int i = 0; i < text.Length - 1; i++)
            {
                double advanceWidth;
                if (typeface.CharacterToGlyphMap.ContainsKey(text[i]))
                {
                    ushort charaIndex = typeface.CharacterToGlyphMap[text[i]];
                    advanceWidth = typeface.AdvanceWidths[charaIndex];
                }
                else
                {
                    advanceWidth = 0.65; // 代わりの字幅
                }
                var fontSize = (double)style.Text.FontSize.Begin;
                var letterSpace = (double)style.Text.LetterSpace;
                var EmMultiplier = 100.0;
                var val = (advanceWidth + letterSpace / fontSize) * EmMultiplier;

                indices += $",{val};";
            }
            return indices;
        }
    }
}
