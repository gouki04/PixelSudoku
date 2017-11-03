using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace BoarderGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            // 5||1 64 1|1 64 1|1 64 1||3||1 64 1|1 64 1|1 64 1||3||1 64 1|1 64 1|1 64 1||5
            // (64 + 1 * 2) * 9 + 5 * 2 + 3 * (3 - 1) == 594 + 10 + 6 == 610
            var cell_size = 64;
            var border = 3;
            var margin = 1;

            var width = (cell_size + margin * 2) * 9 + (border + 2) * 2 + border * (3 - 1);
            var height = width;
            var bitmap = new Bitmap(width, height);

            for (var r = 0; r < width; ++r) {
                for (var c = 0; c < height; ++c) {
                    bitmap.SetPixel(r, c, Color.FromArgb(255, 0, 0, 0));
                }
            }

            for (var gr = 0; gr < 3; ++gr) {
                var r = border + 2 + gr * ((cell_size + margin * 2) * 3 + border);

                for (var gc = 0; gc < 3; ++gc) {
                    var c = border + 2 + gc * ((cell_size + margin * 2) * 3 + border);

                    for (var gr2 = 0; gr2 < 3; ++gr2) {
                        var r2 = r + gr2 * (cell_size + margin * 2);

                        for (var gc2 = 0; gc2 < 3; ++gc2) {
                            var c2 = c + gc2 * (cell_size + margin * 2);

                            for (var y = 0; y < 64; ++y) {
                                for (var x = 0; x < 64; ++x) {
                                    bitmap.SetPixel(r2 + y + margin, c2 + x + margin, Color.FromArgb(255, 255, 255, 255));
                                }
                            }
                        }
                    }
                }
            }

            bitmap.Save("border.png", ImageFormat.Png);
        }
    }
}
