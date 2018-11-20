using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tween the object's alpha. Works with both UI widgets as well as renderers.
/// </summary>

[AddComponentMenu("UI/Tween/Tween Alpha")]
public class TweenAlpha : UITweener
{
	[Range(0f, 1f)] public float from = 1f;
	[Range(0f, 1f)] public float to = 1f;

	bool mCached = false;
    CanvasGroup mCanvasGroup;
    Graphic mRect;
	Material mMat;
	SpriteRenderer mSr;

	[System.Obsolete("Use 'value' instead")]
	public float alpha { get { return this.value; } set { this.value = value; } }

	void Cache ()
	{
		mCached = true;
		mRect = GetComponent<Graphic>();
		mSr = GetComponent<SpriteRenderer>();
        mCanvasGroup = GetComponent<CanvasGroup>();

        if (mRect == null && mSr == null && mCanvasGroup == null)
		{
			Renderer ren = GetComponent<Renderer>();
			if (ren != null) mMat = ren.material;
			if (mMat == null) mRect = GetComponentInChildren<Graphic>();
		}
	}

	/// <summary>
	/// Tween's current value.
	/// </summary>

	public float value
	{
		get
		{
			if (!mCached) Cache();

            if (mCanvasGroup != null)
            {
                return mCanvasGroup.alpha;
            }

            if (mRect != null)
            {
                return mRect.GetAlpha();
            }

			if (mSr != null) return mSr.color.a;
			return mMat != null ? mMat.color.a : 1f;
		}
		set
		{
			if (!mCached) Cache();

            if (mCanvasGroup != null)
            {
                mCanvasGroup.alpha = value;
            }
            else if (mRect != null)
			{
                mRect.SetAlpha(value);
            }
			else if (mSr != null)
			{
				Color c = mSr.color;
				c.a = value;
				mSr.color = c;
			}
			else if (mMat != null)
			{
				Color c = mMat.color;
				c.a = value;
				mMat.color = c;
			}
		}
	}

	/// <summary>
	/// Tween the value.
	/// </summary>

	protected override void OnUpdate (float factor, bool isFinished) { value = Mathf.Lerp(from, to, factor); }

	/// <summary>
	/// Start the tweening operation.
	/// </summary>

	static public TweenAlpha Begin (GameObject go, float duration, float alpha)
	{
		TweenAlpha comp = UITweener.Begin<TweenAlpha>(go, duration);
		comp.from = comp.value;
		comp.to = alpha;

		if (duration <= 0f)
		{
			comp.Sample(1f, true);
			comp.enabled = false;
		}
		return comp;
	}

	public override void SetStartToCurrentValue () { from = value; }
	public override void SetEndToCurrentValue () { to = value; }
}
