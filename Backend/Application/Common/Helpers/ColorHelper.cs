using System.Text.RegularExpressions;

namespace fundoo_notes.Application.Common.Helpers
{
    /// <summary>
    /// Helper class for handling note colors and patterns
    /// </summary>
    public static class ColorHelper
    {
        // Valid pattern identifiers
        public static readonly HashSet<string> ValidPatterns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "grid",
            "lines", 
            "dots",
            "gradient"
        };

        // Valid Google Keep-style colors
        public static readonly HashSet<string> ValidColors = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "#FFF9C4", // keep-yellow
            "#FBBC04", // keep-orange
            "#F28B82", // keep-red
            "#FDCFE8", // keep-pink
            "#D7AEFB", // keep-purple
            "#AECBFA", // keep-blue
            "#A7FFEB", // keep-teal
            "#CCFF90", // keep-green
            "#E6C9A8", // keep-brown
            "#E8EAED", // keep-gray
            "#FFFFFF"  // keep-white (default)
        };

        /// <summary>
        /// Validates if the provided color is a valid hex color or pattern
        /// </summary>
        /// <param name="color">Color string to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidColor(string? color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return true; // Null/empty is valid (defaults to white)

            // Check if it's a valid pattern
            if (ValidPatterns.Contains(color))
                return true;

            // Check if it's a valid predefined color
            if (ValidColors.Contains(color.ToUpperInvariant()))
                return true;

            // Check if it's a valid hex color format
            return IsValidHexColor(color);
        }

        /// <summary>
        /// Checks if the color is a pattern (not a hex color)
        /// </summary>
        /// <param name="color">Color string to check</param>
        /// <returns>True if it's a pattern, false otherwise</returns>
        public static bool IsPattern(string? color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return false;

            return ValidPatterns.Contains(color);
        }

        /// <summary>
        /// Gets the display color for UI rendering
        /// For patterns, returns white (#FFFFFF) for UI display
        /// For regular colors, returns the color itself
        /// </summary>
        /// <param name="color">Stored color value</param>
        /// <returns>Color to display in UI</returns>
        public static string GetDisplayColor(string? color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return "#FFFFFF"; // Default white

            // If it's a pattern, return white for display
            if (IsPattern(color))
                return "#FFFFFF";

            // Return the actual color
            return color;
        }

        /// <summary>
        /// Gets the pattern type if the color is a pattern
        /// </summary>
        /// <param name="color">Color string to check</param>
        /// <returns>Pattern type or null if not a pattern</returns>
        public static string? GetPatternType(string? color)
        {
            if (IsPattern(color))
                return color?.ToLowerInvariant();

            return null;
        }

        /// <summary>
        /// Validates hex color format
        /// </summary>
        /// <param name="color">Color string to validate</param>
        /// <returns>True if valid hex color</returns>
        private static bool IsValidHexColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return false;

            // Check for valid hex color format (#RRGGBB or #RGB)
            var hexPattern = @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$";
            return Regex.IsMatch(color, hexPattern);
        }

        /// <summary>
        /// Normalizes color input - converts to proper format
        /// </summary>
        /// <param name="color">Input color</param>
        /// <returns>Normalized color</returns>
        public static string? NormalizeColor(string? color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return null;

            var trimmedColor = color.Trim();

            // If it's a pattern, return lowercase
            if (ValidPatterns.Contains(trimmedColor))
                return trimmedColor.ToLowerInvariant();

            // If it's a hex color, return uppercase
            if (IsValidHexColor(trimmedColor))
                return trimmedColor.ToUpperInvariant();

            // Check predefined colors (case insensitive)
            var predefinedColor = ValidColors.FirstOrDefault(c => 
                string.Equals(c, trimmedColor, StringComparison.OrdinalIgnoreCase));
            
            if (predefinedColor != null)
                return predefinedColor;

            return trimmedColor; // Return as-is if not recognized
        }
    }
}
