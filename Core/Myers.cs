// Diff Match and Patch
// Copyright 2018 The diff-match-patch Authors.
// https://github.com/google/diff-match-patch
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using Patience.Models;

namespace Patience.Core
{
    internal static class CompatibilityExtensions
    {
        // JScript splice function
        public static List<T> Splice<T>(this List<T> input, int start, int count, params T[] objects)
        {
            List<T> deletedRange = input.GetRange(start, count);
            input.RemoveRange(start, count);
            input.InsertRange(start, objects);
            return deletedRange;
        }
    }

    internal class HalfMatch
    {
        public string Prefix1 { get; set; }
        public string Suffix1 { get; set; }
        public string Prefix2 { get; set; }
        public string Suffix2 { get; set; }
        public string CommonMiddle { get; set; }

        public HalfMatch SwapProperties()
        {
            return new HalfMatch
            {
                Prefix1 = Prefix2,
                Suffix1 = Suffix2,
                Prefix2 = Prefix1,
                Suffix2 = Suffix1,
                CommonMiddle = CommonMiddle
            };
        }
    }

    public class Myers
    {
        /**
         * Find the differences between two texts.
         * Simplifies the problem by stripping any common prefix or suffix off the texts before diffing.
         * @param text1 Old string to be diffed.
         * @param text2 New string to be diffed.
         * @return List of Diff objects.
         */
        public List<Diff> Diff(string text1, string text2)
        {
            // Check for null inputs not needed since null can't be passed in C#.
            // Check for equality (speedup).
            if (text1 == text2)
            {
                var equal = new List<Diff>();
                if (text1.Length != 0)
                {
                    equal.Add(new Diff(Operation.Equal, text1));
                }
                return equal;
            }

            // Trim off common prefix (speedup).
            var commonPrefixLength = GetCommonPrefixLength(text1, text2);
            var commonPrefix = text1.Substring(0, commonPrefixLength);
            text1 = text1.Substring(commonPrefixLength);
            text2 = text2.Substring(commonPrefixLength);

            // Trim off common suffix (speedup).
            var commonSuffixLength = GetCommonSuffixLength(text1, text2);
            var commonSuffix = text1.Substring(text1.Length - commonSuffixLength);
            text1 = text1.Substring(0, text1.Length - commonSuffixLength);
            text2 = text2.Substring(0, text2.Length - commonSuffixLength);

            // Compute the diff on the middle block.
            var diffs = DiffWithoutSuffixCheck(text1, text2);

            // Restore the prefix and suffix.
            if (commonPrefix.Length != 0)
            {
                diffs.Insert(0, new Diff(Operation.Equal, commonPrefix));
            }
            if (commonSuffix.Length != 0)
            {
                diffs.Add(new Diff(Operation.Equal, commonSuffix));
            }

            Cleanup(diffs);
            return diffs;
        }

        /**
         * Find the differences between two texts.
         * Assumes that the texts do not have any common prefix or suffix.
         * @param text1 Old string to be diffed.
         * @param text2 New string to be diffed.
         * @return List of Diff objects.
         */
        private List<Diff> DiffWithoutSuffixCheck(string text1, string text2)
        {

            if (text1.Length == 0)
            {
                // Just add some text (speedup).
                return new List<Diff>(1)
                {
                    new Diff(Operation.Insert, text2)
                };
            }

            if (text2.Length == 0)
            {
                // Just delete some text (speedup).
                return new List<Diff>(1)
                {
                    new Diff(Operation.Delete, text1)
                };
            }

            var longtext = text1.Length > text2.Length ? text1 : text2;
            var shorttext = text1.Length > text2.Length ? text2 : text1;
            var i = longtext.IndexOf(shorttext, StringComparison.Ordinal);
            if (i != -1)
            {
                // Shorter text is inside the longer text (speedup).
                var op = text1.Length > text2.Length ? Operation.Delete : Operation.Insert;
                return new List<Diff>(3)
                {
                    new Diff(op, longtext.Substring(0, i)), 
                    new Diff(Operation.Equal, shorttext), 
                    new Diff(op, longtext.Substring(i + shorttext.Length))
                };
            }

            if (shorttext.Length == 1)
            {
                // Single character string.
                // After the previous speedup, the character can't be an equality.
                return new List<Diff>(2)
                {
                    new Diff(Operation.Delete, text1),
                    new Diff(Operation.Insert, text2)
                };
            }

            // Check to see if the problem can be split in two.
            var hm = HalfMatch(text1, text2);
            if (hm != null)
            {
                // A half-match was found, sort out the return data.
                // Send both pairs off for separate processing.
                var prefixDiffs = Diff(hm.Prefix1, hm.Prefix2);
                var suffixDiffs = Diff(hm.Suffix1, hm.Suffix2);

                // Merge the results.
                var diffs = new List<Diff>();
                diffs.AddRange(prefixDiffs);
                diffs.Add(new Diff(Operation.Equal, hm.CommonMiddle));
                diffs.AddRange(suffixDiffs);
                return diffs;
            }

            return Bisect(text1, text2);
        }

        /**
         * Find the 'middle snake' of a diff, split the problem in two
         * and return the recursively constructed diff.
         * See Myers 1986 paper: An O(ND) Difference Algorithm and Its Variations.
         * @param text1 Old string to be diffed.
         * @param text2 New string to be diffed.
         * @param deadline Time at which to bail if not yet complete.
         * @return List of Diff objects.
         */
        private List<Diff> Bisect(string text1, string text2)
        {
            // Cache the text lengths to prevent multiple calls.
            var text1_length = text1.Length;
            var text2_length = text2.Length;
            var max_d = (text1_length + text2_length + 1) / 2;
            var v_offset = max_d;
            var v_length = 2 * max_d;
            var v1 = new int[v_length];
            var v2 = new int[v_length];
            for (var x = 0; x < v_length; x++)
            {
                v1[x] = -1;
                v2[x] = -1;
            }
            v1[v_offset + 1] = 0;
            v2[v_offset + 1] = 0;
            var delta = text1_length - text2_length;
            // If the total number of characters is odd, then the front path will
            // collide with the reverse path.
            var front = (delta % 2 != 0);
            // Offsets for start and end of k loop.
            // Prevents mapping of space beyond the grid.
            var k1start = 0;
            var k1end = 0;
            var k2start = 0;
            var k2end = 0;
            for (var d = 0; d < max_d; d++)
            {
                // Walk the front path one step.
                for (var k1 = -d + k1start; k1 <= d - k1end; k1 += 2)
                {
                    var k1_offset = v_offset + k1;
                    int x1;
                    if (k1 == -d || k1 != d && v1[k1_offset - 1] < v1[k1_offset + 1])
                    {
                        x1 = v1[k1_offset + 1];
                    }
                    else
                    {
                        x1 = v1[k1_offset - 1] + 1;
                    }
                    var y1 = x1 - k1;
                    while (x1 < text1_length && y1 < text2_length && text1[x1] == text2[y1])
                    {
                        x1++;
                        y1++;
                    }
                    v1[k1_offset] = x1;
                    if (x1 > text1_length)
                    {
                        // Ran off the right of the graph.
                        k1end += 2;
                    }
                    else if (y1 > text2_length)
                    {
                        // Ran off the bottom of the graph.
                        k1start += 2;
                    }
                    else if (front)
                    {
                        var k2_offset = v_offset + delta - k1;
                        if (k2_offset >= 0 && k2_offset < v_length && v2[k2_offset] != -1)
                        {
                            // Mirror x2 onto top-left coordinate system.
                            var x2 = text1_length - v2[k2_offset];
                            if (x1 >= x2)
                            {
                                // Overlap detected.
                                return Split(text1, text2, x1, y1);
                            }
                        }
                    }
                }

                // Walk the reverse path one step.
                for (var k2 = -d + k2start; k2 <= d - k2end; k2 += 2)
                {
                    var k2_offset = v_offset + k2;
                    int x2;
                    if (k2 == -d || k2 != d && v2[k2_offset - 1] < v2[k2_offset + 1])
                    {
                        x2 = v2[k2_offset + 1];
                    }
                    else
                    {
                        x2 = v2[k2_offset - 1] + 1;
                    }
                    var y2 = x2 - k2;
                    while (
                        x2 < text1_length && 
                        y2 < text2_length && 
                        text1[text1_length - x2 - 1] == text2[text2_length - y2 - 1])
                    {
                        x2++;
                        y2++;
                    }
                    v2[k2_offset] = x2;
                    if (x2 > text1_length)
                    {
                        // Ran off the left of the graph.
                        k2end += 2;
                    }
                    else if (y2 > text2_length)
                    {
                        // Ran off the top of the graph.
                        k2start += 2;
                    }
                    else if (!front)
                    {
                        var k1_offset = v_offset + delta - k2;
                        if (k1_offset >= 0 && k1_offset < v_length && v1[k1_offset] != -1)
                        {
                            var x1 = v1[k1_offset];
                            var y1 = v_offset + x1 - k1_offset;
                            // Mirror x2 onto top-left coordinate system.
                            x2 = text1_length - v2[k2_offset];
                            if (x1 >= x2)
                            {
                                // Overlap detected.
                                return Split(text1, text2, x1, y1);
                            }
                        }
                    }
                }
            }
            // Diff took too long and hit the deadline or
            // number of diffs equals number of characters, no commonality at all.
            return new List<Diff>
            {
                new Diff(Operation.Delete, text1), 
                new Diff(Operation.Insert, text2)
            };
        }

        /**
         * Given the location of the 'middle snake', split the diff in two parts
         * and recurse.
         * @param text1 Old string to be diffed.
         * @param text2 New string to be diffed.
         * @param x Index of split point in text1.
         * @param y Index of split point in text2.
         * @return LinkedList of Diff objects.
         */
        private List<Diff> Split(string text1, string text2, int x, int y)
        {
            var text1a = text1.Substring(0, x);
            var text2a = text2.Substring(0, y);
            var text1b = text1.Substring(x);
            var text2b = text2.Substring(y);

            // Compute both diffs serially.
            var diffs = Diff(text1a, text2a);
            var diffsb = Diff(text1b, text2b);

            diffs.AddRange(diffsb);
            return diffs;
        }

        /**
         * Determine the common prefix of two strings.
         * @param text1 First string.
         * @param text2 Second string.
         * @return The number of characters common to the start of each string.
         */
        public int GetCommonPrefixLength(string text1, string text2)
        {
            // Performance analysis: https://neil.fraser.name/news/2007/10/09/
            var maxIndex = Math.Min(text1.Length, text2.Length);
            for (var i = 0; i < maxIndex; i++)
            {
                if (text1[i] != text2[i])
                {
                    return i;
                }
            }
            return maxIndex;
        }

        /**
         * Determine the common suffix of two strings.
         * @param text1 First string.
         * @param text2 Second string.
         * @return The number of characters common to the end of each string.
         */
        public int GetCommonSuffixLength(string text1, string text2)
        {
            // Performance analysis: https://neil.fraser.name/news/2007/10/09/
            var length1 = text1.Length;
            var length2 = text2.Length;
            var maxIndex = Math.Min(text1.Length, text2.Length);
            for (var i = 1; i <= maxIndex; i++)
            {
                if (text1[length1 - i] != text2[length2 - i])
                {
                    return i - 1;
                }
            }
            return maxIndex;
        }

        /**
         * Do the two texts share a Substring which is at least half the length of
         * the longer text?
         * This speedup can produce non-minimal diffs.
         * @param text1 First string.
         * @param text2 Second string.
         * @return Five element String array, containing the prefix of text1, the
         *     suffix of text1, the prefix of text2, the suffix of text2 and the
         *     common middle.  Or null if there was no match.
         */
        private HalfMatch HalfMatch(string text1, string text2)
        {
            var longtext = text1.Length > text2.Length ? text1 : text2;
            var shorttext = text1.Length > text2.Length ? text2 : text1;
            if (longtext.Length < 4 || shorttext.Length * 2 < longtext.Length)
            {
                return null; // Pointless.
            }

            // First check if the second quarter is the seed for a half-match.
            var hm1 = HalfMatchInQuarter(longtext, shorttext, (longtext.Length + 3) / 4);

            // Check again based on the third quarter.
            var hm2 = HalfMatchInQuarter(longtext, shorttext, (longtext.Length + 1) / 2);
            
            HalfMatch hm;
            if (hm1 == null && hm2 == null)
            {
                return null;
            }
            else if (hm2 == null)
            {
                hm = hm1;
            }
            else if (hm1 == null)
            {
                hm = hm2;
            }
            else
            {
                // Both matched. Select the longest.
                hm = hm1.CommonMiddle.Length > hm2.CommonMiddle.Length ? hm1 : hm2;
            }

            // A half-match was found, sort out the return data.
            if (text1.Length > text2.Length)
            {
                return hm;
            }

            // if text2 > text1, swap properties
            return hm.SwapProperties();
        }

        /**
         * Does a Substring of shorttext exist within longtext such that the
         * Substring is at least half the length of longtext?
         * @param longtext Longer string.
         * @param shorttext Shorter string.
         * @param i Start index of quarter length Substring within longtext.
         * @return Five element string array, containing the prefix of longtext, the
         *     suffix of longtext, the prefix of shorttext, the suffix of shorttext
         *     and the common middle.  Or null if there was no match.
         */
        private HalfMatch HalfMatchInQuarter(string longtext, string shorttext, int i)
        {
            // Start with a 1/4 length Substring at position i as a seed.
            var seed = longtext.Substring(i, longtext.Length / 4);
            var j = -1;
            var bestCommon = string.Empty;
            string best_longtext_a = string.Empty, best_longtext_b = string.Empty;
            string best_shorttext_a = string.Empty, best_shorttext_b = string.Empty;
            while (j < shorttext.Length && (j = shorttext.IndexOf(seed, j + 1, StringComparison.Ordinal)) != -1)
            {
                var prefixLength = GetCommonPrefixLength(longtext.Substring(i), shorttext.Substring(j));
                var suffixLength = GetCommonSuffixLength(longtext.Substring(0, i), shorttext.Substring(0, j));
                if (bestCommon.Length < suffixLength + prefixLength)
                {
                    bestCommon = shorttext.Substring(j - suffixLength, suffixLength) + shorttext.Substring(j, prefixLength);
                    best_longtext_a = longtext.Substring(0, i - suffixLength);
                    best_longtext_b = longtext.Substring(i + prefixLength);
                    best_shorttext_a = shorttext.Substring(0, j - suffixLength);
                    best_shorttext_b = shorttext.Substring(j + prefixLength);
                }
            }

            if (bestCommon.Length * 2 < longtext.Length)
            {
                return null;
            }

            return new HalfMatch
            {
                Prefix1 = best_longtext_a,
                Suffix1 = best_longtext_b,
                Prefix2 = best_shorttext_a,
                Suffix2 = best_shorttext_b,
                CommonMiddle = bestCommon
            };
        }

        /**
         * Reorder and merge like edit sections.  Merge equalities.
         * Any edit section can move as long as it doesn't cross an equality.
         * @param diffs List of Diff objects.
         */
        private void Cleanup(List<Diff> diffs)
        {
            // Add a dummy entry at the end.
            diffs.Add(new Diff(Operation.Equal, string.Empty));
            var pointer = 0;
            var count_delete = 0;
            var count_insert = 0;
            var text_delete = string.Empty;
            var text_insert = string.Empty;
            while (pointer < diffs.Count)
            {
                switch (diffs[pointer].Operation)
                {
                    case Operation.Insert:
                        count_insert++;
                        text_insert += diffs[pointer].Text;
                        pointer++;
                        break;
                    case Operation.Delete:
                        count_delete++;
                        text_delete += diffs[pointer].Text;
                        pointer++;
                        break;
                    case Operation.Equal:
                        // Upon reaching an equality, check for prior redundancies.
                        if (count_delete + count_insert > 1)
                        {
                            if (count_delete != 0 && count_insert != 0)
                            {
                                // Factor out any common prefixies.
                                var commonlength = GetCommonPrefixLength(text_insert, text_delete);
                                if (commonlength != 0)
                                {
                                    if (pointer - count_delete - count_insert > 0 && 
                                        diffs[pointer - count_delete - count_insert - 1].Operation == Operation.Equal)
                                    {
                                        diffs[pointer - count_delete - count_insert - 1].Append(text_insert.Substring(0, commonlength));
                                    }
                                    else
                                    {
                                        diffs.Insert(0, new Diff(Operation.Equal, text_insert.Substring(0, commonlength)));
                                        pointer++;
                                    }
                                    text_insert = text_insert.Substring(commonlength);
                                    text_delete = text_delete.Substring(commonlength);
                                }
                                // Factor out any common suffixies.
                                commonlength = GetCommonSuffixLength(text_insert, text_delete);
                                if (commonlength != 0)
                                {
                                    diffs[pointer].Prepend(text_insert.Substring(text_insert.Length - commonlength));
                                    text_insert = text_insert.Substring(0, text_insert.Length - commonlength);
                                    text_delete = text_delete.Substring(0, text_delete.Length - commonlength);
                                }
                            }
                            // Delete the offending records and add the merged ones.
                            pointer -= count_delete + count_insert;
                            diffs.Splice(pointer, count_delete + count_insert);
                            if (text_delete.Length != 0)
                            {
                                diffs.Splice(pointer, 0, new Diff(Operation.Delete, text_delete));
                                pointer++;
                            }
                            if (text_insert.Length != 0)
                            {
                                diffs.Splice(pointer, 0, new Diff(Operation.Insert, text_insert));
                                pointer++;
                            }
                            pointer++;
                        }
                        else if (pointer != 0 && diffs[pointer - 1].Operation == Operation.Equal)
                        {
                            // Merge this equality with the previous one.
                            diffs[pointer - 1].Append(diffs[pointer].Text);
                            diffs.RemoveAt(pointer);
                        }
                        else
                        {
                            pointer++;
                        }
                        count_insert = 0;
                        count_delete = 0;
                        text_delete = string.Empty;
                        text_insert = string.Empty;
                        break;
                }
            }
            if (diffs[diffs.Count - 1].Text.Length == 0)
            {
                diffs.RemoveAt(diffs.Count - 1);  // Remove the dummy entry at the end.
            }

            // Second pass: look for single edits surrounded on both sides by
            // equalities which can be shifted sideways to eliminate an equality.
            // e.g: A<ins>BA</ins>C -> <ins>AB</ins>AC
            var changes = false;
            pointer = 1;
            // Intentionally ignore the first and last element (don't need checking).
            while (pointer < diffs.Count - 1)
            {
                if (diffs[pointer - 1].Operation == Operation.Equal && 
                    diffs[pointer + 1].Operation == Operation.Equal)
                {
                    // This is a single edit surrounded by equalities.
                    if (diffs[pointer].Text.EndsWith(diffs[pointer - 1].Text, StringComparison.Ordinal))
                    {
                        // Shift the edit over the previous equality.
                        diffs[pointer].Update(
                            diffs[pointer - 1].Text + 
                            diffs[pointer].Text.Substring(0, diffs[pointer].Text.Length - diffs[pointer - 1].Text.Length));

                        // b = a + b.Substring(0, b.Length - a.Length)
                        // c = a + c

                        diffs[pointer + 1].Prepend(diffs[pointer - 1].Text);
                        diffs.Splice(pointer - 1, 1);
                        changes = true;
                    }
                    else if (diffs[pointer].Text.StartsWith(diffs[pointer + 1].Text, StringComparison.Ordinal))
                    {
                        // Shift the edit over the next equality.
                        diffs[pointer - 1].Append(diffs[pointer + 1].Text);
                        diffs[pointer].Update(
                            diffs[pointer].Text.Substring(diffs[pointer + 1].Text.Length) + 
                            diffs[pointer + 1].Text);
                        
                        // a = a + c
                        // b = b.Substring(c.Length) + c

                        diffs.Splice(pointer + 1, 1);
                        changes = true;
                    }
                }
                pointer++;
            }

            // If shifts were made, the diff needs reordering and another shift sweep.
            if (changes)
            {
                Cleanup(diffs);
            }
        }
    }
}
