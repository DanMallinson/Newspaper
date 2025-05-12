# Newspaper
System for parsing RSS newsfeeds / blogs and collating them into a file format for offline reading

# Usage

Create a JSON file in the following format:

{
  "Name"                  : "",                    -- The name of the newspaper
  "MainImage"             : "",                    -- Path to an image to use as a letterhead
  "Frequency"             : "D.DD:HH:mm:SS",       -- Timespan for how frequent the letter runs (how far back to pull articles)
  "RenderImages"          : false/true,            -- Whether images found in articles should be rendered in the PDF
  "Categories"            : \[                     -- Array of categorised sources
    {
      "Name"              : "",                    -- The title of the category
      "Sources"           : \[                     -- Array of article sources
        {
          "Name"          : "",                    -- The title of the source
          "Url"           : "",                    -- The Url of the RSS feed
          "IncludeTables" : false/true,            -- Whether tables found in the article should be rendered in the PDF
          "IncludeImages" : false/true,            -- Whether images found in the article should be rendered in the PDF
          "Timeout"       : 30000,                 -- The timeout for getting the RSS feed information
          "Includes"      : \[                     -- List of strings. Represents the RSS categories to only include (articles will be ignored if a category is not listed)
          \],
          "Excludes"      : \[                     -- List of strings. Represents the RSS categories to only include (articles will be ignored if a category is listed)
          \],
          "ParseOrder"    : \[                     -- Order in which to prioritise RSS feed content
            0,                                     -- Content node
            1,                                     -- Link node
            2,                                     -- Description node
          \],
          "ContentNode" : {                        -- Used for stripping down content when retrieving it from a link / html page
            "NodeType:" "",                        -- The type of html node to extract content from
            "PropertyName": "",                    -- The name of the property used to filter the node
            "PropertyValue": "",                   -- The value assigned to the property, used to filter the node
          }
        }
      \]
    }
  \]
}
