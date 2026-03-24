using System;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace PuzzleGame.Screens;

internal static class TextLayout
{
    public static string WrapText(SpriteFont font, string text, float maxLineWidth)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }
        var normalizedText = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        var paragraphs = normalizedText.Split('\n');
        var builder = new StringBuilder();
        var spaceWidth = font.MeasureString(" ").X;
        for (var paragraphIndex = 0; paragraphIndex < paragraphs.Length; paragraphIndex++)
        {
            var paragraph = paragraphs[paragraphIndex];
            if (string.IsNullOrWhiteSpace(paragraph))
            {
                if (paragraphIndex < paragraphs.Length - 1)
                {
                    builder.AppendLine();
                }
                continue;
            }
            var lineWidth = 0f;
            foreach (var word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var wordWidth = font.MeasureString(word).X;
                if (lineWidth > 0f && lineWidth + spaceWidth + wordWidth > maxLineWidth)
                {
                    builder.AppendLine();
                    lineWidth = 0f;
                }
                if (lineWidth > 0f)
                {
                    builder.Append(' ');
                    lineWidth += spaceWidth;
                }
                builder.Append(word);
                lineWidth += wordWidth;
            }
            if (paragraphIndex < paragraphs.Length - 1)
            {
                builder.AppendLine();
            }
        }
        return builder.ToString().TrimEnd();
    }
}