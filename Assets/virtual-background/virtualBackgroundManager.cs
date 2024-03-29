using Agora.Rtc;
using UnityEngine;

public class VirtualBackgroundManager : AuthenticationWorkflowManager
{
    public void setVirtualBackground(bool enableVirtualBackgroud, string option)
    {
        if(agoraEngine == null)
        {
            Debug.Log("Please join a channel to enable virtual background");
            return;
        }
        VirtualBackgroundSource virtualBackgroundSource = new VirtualBackgroundSource();

        // Set the type of virtual background
        if (option ==  "Blur")
        { // Set background blur
            virtualBackgroundSource.background_source_type = BACKGROUND_SOURCE_TYPE.BACKGROUND_BLUR;
            virtualBackgroundSource.blur_degree = BACKGROUND_BLUR_DEGREE.BLUR_DEGREE_HIGH;
            Debug.Log("Blur background enabled");
        }
        else if (option == "Color")
        { // Set a solid background color
            virtualBackgroundSource.background_source_type = BACKGROUND_SOURCE_TYPE.BACKGROUND_COLOR;
            virtualBackgroundSource.color = 0x0000FF;
            Debug.Log("Color background enabled");
        }
        else if (option ==  "Image")
        { // Set a background image
            virtualBackgroundSource.background_source_type = BACKGROUND_SOURCE_TYPE.BACKGROUND_IMG;
            virtualBackgroundSource.source = "Assets/Resources/agora.png";
            Debug.Log("Image background enabled");
        }

        // Set processing properties for background
        SegmentationProperty segmentationProperty = new SegmentationProperty();
        segmentationProperty.modelType = SEG_MODEL_TYPE.SEG_MODEL_AI; // Use SEG_MODEL_GREEN if you have a green background
        segmentationProperty.greenCapacity = 0.5F; // Accuracy for identifying green colors (range 0-1)

        // Enable or disable virtual background
        agoraEngine.EnableVirtualBackground(enableVirtualBackgroud, virtualBackgroundSource, segmentationProperty);
    }

}
