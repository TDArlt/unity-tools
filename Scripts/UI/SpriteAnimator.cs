using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpriteAnimator : MonoBehaviour
{
    // ######################## ENUMS & DELEGATES ######################## //


    // ######################## LINKS TO UNITY OBJECTS ######################## //

    /// <summary>This is the list of sprites that are used here</summary>
    public Sprite[] AnimationSprites;

    /// <summary>This is what should be shown when inactive</summary>
    public Sprite ImageForInactive;

    // ######################## PUBLIC VARS ######################## //

    /// <summary>This is the speed of this animation</summary>
    public float SecsPerSprite = 0.25f;

    /// <summary>Defines, if this is currently active (if not, inactive image will be shown)</summary>
    public bool CurrentlyActive = false;

    /// <summary>Defines, if this one should run in reverse direction</summary>
    public bool Reverse = false;



    // ######################## PRIVATE VARS ######################## //

    /// <summary>The link to the image component</summary>
    private Image image;


    /// <summary>The current sprite id. Is -1, if on "inactive"</summary>
    private int currentSpriteID;


    // ######################## UNITY START & UPDATE ######################## //
    
    void Awake() { Init(); }
    

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnEnable()
    {
        StartCoroutine(AnimateSprite());
    }

    // ######################## INITS ######################## //

    /// <summary>Does the init for this behaviour</summary>
    private void Init()
    {
        image = GetComponent<Image>();
        currentSpriteID = 0;
        image.sprite = AnimationSprites[0];
    }


    // ######################## FUNCTIONALITY ######################## //



    // ######################## COROUTINES ######################## //

    private IEnumerator AnimateSprite()
    {
        float time;
        while (true)
        {
            if (!CurrentlyActive && currentSpriteID > -1)
            {
                currentSpriteID = -1;
                image.sprite = ImageForInactive;
            }
            else if (currentSpriteID == -1 && CurrentlyActive)
                currentSpriteID = 0;



            if (currentSpriteID > -1)
            {
                time = Time.time;
                yield return new WaitForSeconds(SecsPerSprite);

                if (!Reverse)
                {
                    currentSpriteID = (currentSpriteID + Mathf.FloorToInt((Time.time - time) / SecsPerSprite)) % AnimationSprites.Length;
                } else
                {
                    currentSpriteID = (AnimationSprites.Length + currentSpriteID - Mathf.FloorToInt((Time.time - time) / SecsPerSprite)) % AnimationSprites.Length;
                }
                image.sprite = AnimationSprites[Mathf.RoundToInt(Mathf.Clamp(currentSpriteID, 0, AnimationSprites.Length - 1))];
            } else
            {
                yield return null;
            }
        }
    }


    // ######################## UTILITIES ######################## //


}
