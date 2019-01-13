using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

using InfinitySynergy.Utility.Types;
using InfiniteSynergy.MobileForHero.BusinessServices.Contracts.BusinessProcess;
using InfiniteSynergy.MobileForHero.BusinessServices.ActionParams.BusinessProcess.CaseRenderingService;

namespace InfinitySynerge.MobileForHero.BusinessServices.BusinesProcess
{
    public class CaseRenderingService : ICaseRenderingService
    {
        private const int OffsetWidth = 2;
        private const int OffsetHeight = 2;

        private const string PNGExtension = "png";

        public MethodResult<RenderResponse> Render(RenderRequest request)
        {
            var response = default(RenderResponse);

            var blankSize = new Rectangle(0, 0, request.BlankSize.Width, request.BlankSize.Height);
            var blankCaseSize = new Rectangle(request.BlankCaseSize.Offset.X, request.BlankCaseSize.Offset.Y, request.BlankCaseSize.Width, request.BlankCaseSize.Height);
            var blankCaseWithBackgroundSize = new Rectangle(request.BlankCaseWithBackgroundSize.Offset.X, request.BlankCaseWithBackgroundSize.Offset.Y, request.BlankCaseWithBackgroundSize.Width, request.BlankCaseWithBackgroundSize.Height);

            using (var blank = Image.FromFile(request.BlankImageUrl))
            {
                using (var blankCase = Image.FromFile(request.BlankCaseImageUrl))
                {
                    using (var source = new Bitmap(request.BlankSize.Width, request.BlankSize.Height))
                    {
                        using (var sourceCanvas = Graphics.FromImage(source))
                        {
                            SetHighQualityToCanvas(sourceCanvas);

                            if (request.IsBlankRotated)
                            {
                                blank.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            }

                            sourceCanvas.DrawImage(blank, blankSize);
                            sourceCanvas.DrawImage(blankCase, blankCaseWithBackgroundSize);
                            sourceCanvas.Save();

                            using (var target = new Bitmap(request.BlankCaseSize.Width + 2 * OffsetWidth, request.BlankCaseSize.Height + 2 * OffsetHeight))
                            {
                                using (var targetCanvas = Graphics.FromImage(target))
                                {
                                    var targetArea = new Rectangle(0, 0, target.Width, target.Height);
                                    var targetAreaFromFullCanvas = default(Rectangle);

                                    SetHighQualityToCanvas(targetCanvas);

                                    if (request.Mode == RenderMode.CaseAndBlank)
                                    {
                                        targetAreaFromFullCanvas = new Rectangle(
                                            request.BlankCaseSize.Offset.X - OffsetWidth, request.BlankCaseSize.Offset.Y - OffsetHeight,
                                            target.Width, target.Height);
                                    }
                                    else
                                    {
                                        var offsetX = (request.BlankCaseWithBackgroundSize.Width - request.BlankCaseSize.Width) / 2;
                                        var offsetY = (request.BlankCaseWithBackgroundSize.Height - request.BlankCaseSize.Height) / 2;

                                        targetAreaFromFullCanvas = new Rectangle(
                                            request.BlankCaseWithBackgroundSize.Offset.X + offsetX - OffsetWidth,
                                            request.BlankCaseWithBackgroundSize.Offset.Y + offsetY - OffsetHeight,
                                            target.Width, target.Height);
                                    }

                                    targetCanvas.DrawImage(source, targetArea, targetAreaFromFullCanvas, GraphicsUnit.Pixel);
                                    targetCanvas.Save();

                                    using (var stream = new MemoryStream())
                                    {
                                        target.Save(stream, ImageFormat.Png);
                                        response = new RenderResponse(GetDataURL(stream.ToArray(), PNGExtension));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return new MethodResult<RenderResponse>(response);
        }

        private string GetDataURL(byte[] imgBytes, string extension)
        {
            return $"data:image/{ extension };base64,{ Convert.ToBase64String(imgBytes) }";
        }

        private void SetHighQualityToCanvas(Graphics canvas)
        {
            canvas.SmoothingMode = SmoothingMode.HighQuality;
            canvas.CompositingQuality = CompositingQuality.HighQuality;
            canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
        }
    }
}
