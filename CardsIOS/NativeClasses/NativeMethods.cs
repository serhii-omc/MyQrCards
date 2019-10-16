using System;
using UIKit;

namespace CardsIOS.NativeClasses
{
    public class NativeMethods
    {
        public static void ClearFeatures()
        {
            QRViewController.ExtraEmploymentData = null;
            QRViewController.CompanyLogoInQr = null;
            QRViewController.ExtraPersonData = null;
        }

        public void DisableFloatingTextField(FloatingTextField textField)
        {
            var inactiveColor = UIColor.FromRGBA(146, 150, 155, 80);
            textField.LabelActiveTextColor = inactiveColor;
            textField.LineColor = inactiveColor;
            textField.LabelInactiveTextColor = inactiveColor;
            textField.TextColor = inactiveColor;
            textField.UserInteractionEnabled = false;
        }

        public void SqueezeImage(ref int maxWidth,
                                  ref int maxHeight,
                                  ref nfloat widthOriginal,
                                  ref nfloat heightOriginal)
        {
            nfloat aspectRatio;
            if (maxWidth > maxHeight)
            {
                aspectRatio = widthOriginal / heightOriginal;
                maxWidth = (int)(maxHeight * aspectRatio);
            }
            else
            {
                aspectRatio = heightOriginal / widthOriginal;
                maxHeight = (int)(maxWidth * aspectRatio);
            }
        }
    }
}
