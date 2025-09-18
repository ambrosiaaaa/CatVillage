using UnityEngine;
using TMPro;

public class Enviro_SunMoon : MonoBehaviour
{
    public GameObject sunObject;
    public GameObject moonObject;
    public GameObject starsObject; // Add this for stars
    public float distance = 1000f; // Distance from the light

    public float dayLengthSeconds = 600f; // 10 minutes
    public float xAxisDegrees = 360f;     // Full rotation for one day
    public float yAxisDegrees = 360f;     // Set to 360f if you want Y to rotate too

    private float xAngle = 0f;
    private float yAngle = 0f;
    public float timeOfDay = 0f; // seconds elapsed in current game day

    public float currentHour = 0f;
    public float currentMinute = 0f;

    public bool isMorning = false;
    public bool isAfternoon = false;
    public bool isNoon = false;
    public bool isMidnight = false;
    public bool isNight = false;
    public bool isDay = false;

    public float starsFadeSpeed = 1f; // Speed of fading in/out
    public float moonFadeSpeed = 1f; // Speed of moon fading in/out

    private CanvasGroup starsCanvasGroup; // For fading (if using UI)
    private Renderer starsRenderer;       // For fading (if using 3D object)
    private Renderer moonRenderer;
    private float starsAlpha = 0f;
    private float moonAlpha = 0f;

    private Color currentFogColor;

    public enum TimeSpeed
    {
        Normal = 1,
        Double = 2,
        FiveTimes = 5,
        TenTimes = 10,
        TwentyTimes = 20,
        FiftyTimes = 50
    }

    [Header("Time Speed (for testing)")]
    public TimeSpeed timeSpeed = TimeSpeed.Normal;

    public TMPro.TextMeshProUGUI timeText; // Reference to your UI text

    [Header("Fog Colors")]
    public Color dayFogColor = new Color(0.376f, 0.365f, 0.302f); // #605D4D
    public Color nightFogColor = new Color(0f, 0.02f, 0.067f);    // #000511

    public float fogFadeSpeed = 1f; // Speed of fog color transition

    // Start is called before the first frame update
    void Start()
    {
        // Start the day at 6am
        timeOfDay = (6f / 24f) * dayLengthSeconds;

        // Get renderer or canvas group for stars
        if (starsObject != null)
        {
            starsCanvasGroup = starsObject.GetComponent<CanvasGroup>();
            starsRenderer = starsObject.GetComponent<Renderer>();
        }

        // Get renderer for moon
        if (moonObject != null)
        {
            moonRenderer = moonObject.GetComponent<Renderer>();
        }
        currentFogColor = RenderSettings.fogColor;
    }

    // Update is called once per frame
    void Update()
    {
        // Advance time with speed multiplier
        float speedMultiplier = (float)timeSpeed;
        timeOfDay += Time.deltaTime * speedMultiplier;
        if (timeOfDay > dayLengthSeconds)
            timeOfDay -= dayLengthSeconds; // Loop back to start of day

        // Sun position: in front of the directional light
        if (sunObject != null)
            sunObject.transform.localPosition = Vector3.back * distance;

        // Moon position: opposite direction
        if (moonObject != null)
            moonObject.transform.localPosition = Vector3.forward * distance;

        SunRotation();
        DisplayGameTime();
        HandleStarsFade();
        HandleMoonFade(); // Add this line
        UpdateTimeUI(); // Add this line

        Color targetFog = isDay ? dayFogColor : nightFogColor;
        currentFogColor = Color.Lerp(currentFogColor, targetFog, Time.deltaTime * fogFadeSpeed);
        RenderSettings.fogColor = currentFogColor;

        // Sun intensity: fade smoothly, stays bright until night
        Light sunLight = GetComponent<Light>();
        if (sunLight != null)
        {
            // Sun fades out only after 20:00 (8pm), stays bright until then
            float hours = currentHour;
            float fadeStart = 20f;
            float fadeEnd = 22f;
            float targetIntensity = 0.4f;

            if (hours >= fadeStart && hours < fadeEnd)
            {
                targetIntensity = Mathf.Lerp(1f, 0.01f, Mathf.InverseLerp(fadeStart, fadeEnd, hours));
            }
            else if (hours >= fadeEnd || hours < 5f)
            {
                targetIntensity = 0.01f;
            }
            else
            {
                targetIntensity = 0.4f;
            }

            sunLight.intensity = Mathf.Lerp(sunLight.intensity, targetIntensity, Time.deltaTime * 2f);
        }
    }

    void SunRotation()
    {
        float speedMultiplier = (float)timeSpeed;
        float xSpeed = (xAxisDegrees / dayLengthSeconds) * speedMultiplier;
        float ySpeed = (yAxisDegrees / dayLengthSeconds) * speedMultiplier;

        xAngle += xSpeed * Time.deltaTime;
        yAngle += ySpeed * Time.deltaTime;

        transform.rotation = Quaternion.Euler(xAngle, yAngle, 0f);
    }

    void DisplayGameTime()
    {
        // Map timeOfDay to 24 hours
        float hours = (timeOfDay / dayLengthSeconds) * 24f;
        int hour = Mathf.FloorToInt(hours);
        int minute = Mathf.FloorToInt((hours - hour) * 60f);

        currentHour = hours;
        currentMinute = (hours - hour) * 60f;

        // Time triggers
        isMorning = (hours >= 5f && hours < 12f);
        isNoon = (hours >= 12f && hours < 17f);
        isAfternoon = (hours >= 17f && hours < 24f);
        isMidnight = (hours >= 0f && hours < 5f);
        isNight = (hours >= 17f || hours < 5f);
        isDay = (hours >= 5f && hours < 17f);

        // Example: print to console (replace with UI as needed)
        //Debug.Log(string.Format("Game Time: {0:00}:{1:00}", hour, minute));
    }

    void HandleStarsFade()
    {
        if (starsObject == null)
            return;

        // Stars fade in from 20:00 (8pm) to 22:00 (10pm), fade out from 3:00 (3am) to 5:00 (5am)
        float fadeInStart = 20f;
        float fadeInEnd = 22f;
        float fadeOutStart = 3f;
        float fadeOutEnd = 5f;

        float targetAlpha = 0f;
        if (currentHour >= fadeInStart && currentHour < fadeInEnd)
        {
            // Fade in
            targetAlpha = Mathf.InverseLerp(fadeInStart, fadeInEnd, currentHour);
        }
        else if (currentHour >= fadeOutStart && currentHour < fadeOutEnd)
        {
            // Fade out
            targetAlpha = 1f - Mathf.InverseLerp(fadeOutStart, fadeOutEnd, currentHour);
        }
        else if ((currentHour >= fadeInEnd && currentHour < 24f) || (currentHour >= 0f && currentHour < fadeOutStart))
        {
            // Fully visible at night
            targetAlpha = 1f;
        }
        else
        {
            // Fully invisible during day
            targetAlpha = 0f;
        }

        starsAlpha = Mathf.MoveTowards(starsAlpha, targetAlpha, starsFadeSpeed * Time.deltaTime);

        // If using CanvasGroup (for UI stars)
        if (starsCanvasGroup != null)
        {
            starsCanvasGroup.alpha = starsAlpha;
            starsCanvasGroup.interactable = starsAlpha > 0.5f;
            starsCanvasGroup.blocksRaycasts = starsAlpha > 0.5f;
        }
        // If using Renderer (for 3D stars object)
        else if (starsRenderer != null && starsRenderer.material.HasProperty("_Color"))
        {
            Color color = starsRenderer.material.color;
            color.a = starsAlpha;
            starsRenderer.material.color = color;
        }
        // Enable/disable stars object based on visibility
        starsObject.SetActive(starsAlpha > 0.01f);
    }

    void HandleMoonFade()
    {
        if (moonObject == null || moonRenderer == null || !moonRenderer.material.HasProperty("_Color"))
            return;

        // Fade in during night, fade out during day, but never fully disappear
        float minAlpha = 0.2f; // Minimum alpha when faded out
        float targetAlpha = isNight ? 1f : minAlpha;
        moonAlpha = Mathf.MoveTowards(moonAlpha, targetAlpha, moonFadeSpeed * Time.deltaTime);

        Color color = moonRenderer.material.color;
        color.a = moonAlpha;
        moonRenderer.material.color = color;

        moonObject.SetActive(true); // Always active
    }

    void UpdateTimeUI()
    {
        if (timeText == null) return;

        int hour = GetHour();
        int minute = GetMinute();

        // Determine AM/PM and convert to 12-hour format
        string ampm = hour < 12 ? "am" : "pm";
        int displayHour = hour % 12;
        if (displayHour == 0) displayHour = 12;

        timeText.text = string.Format("{0:00}:{1:00} {2}", displayHour, minute, ampm);
    }

    // Getters
    public int GetHour()
    {
        return Mathf.FloorToInt(currentHour);
    }

    public int GetMinute()
    {
        return Mathf.FloorToInt(currentMinute);
    }

    public string GetDayPhase()
    {
        if (isMorning) return "Morning";
        if (isNoon) return "Noon";
        if (isAfternoon) return "Afternoon";
        if (isMidnight) return "Midnight";
        return "Unknown";
    }

    public string GetDayOrNight()
    {
        return isDay ? "Day" : "Night";
    }
}
