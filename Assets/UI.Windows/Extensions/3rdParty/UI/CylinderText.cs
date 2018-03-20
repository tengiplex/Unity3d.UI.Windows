﻿/// adaption for cylindrical bending by herbst
/// Credit Breyer
/// Sourced from - http://forum.unity3d.com/threads/scripts-useful-4-6-scripts-collection.264161/#post-1777407

namespace UnityEngine.UI.Windows.Extensions
{
	[RequireComponent(typeof(Text), typeof(RectTransform))]
	[AddComponentMenu("UI/Effects/Extensions/Cylinder Text")]
	public class CylinderText : BaseMeshEffect
	{
		public float radius;
		private RectTransform rectTrans;
		
		
		#if UNITY_EDITOR
		protected override void OnValidate()
		{

			if (GUI.changed == false) return;
			if (Application.isPlaying == true) return;
			#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isUpdating == true) return;
			#endif
			
			base.OnValidate();
			if (rectTrans == null)
				rectTrans = GetComponent<RectTransform>();
		}
		#endif
		protected override void Awake()
		{
			base.Awake();
			rectTrans = GetComponent<RectTransform>();
			OnRectTransformDimensionsChange();
		}
		protected override void OnEnable()
		{
			base.OnEnable();
			rectTrans = GetComponent<RectTransform>();
			OnRectTransformDimensionsChange();
		}
		public override void ModifyMesh(VertexHelper vh)
		{
			if (! IsActive()) return;
			
			int count = vh.currentVertCount;
			if (!IsActive() || count == 0)
			{
				return;
			}
			for (int index = 0; index < vh.currentVertCount; index++)
			{
				UIVertex uiVertex = new UIVertex();
				vh.PopulateUIVertex(ref uiVertex, index);
				
				// get x position
				var x = uiVertex.position.x;                
				
				// calculate bend based on pivot and radius
				uiVertex.position.z = -radius * Mathf.Cos(x / radius);
				uiVertex.position.x = radius * Mathf.Sin(x / radius);
				
				vh.SetUIVertex(uiVertex, index);
			}
		}
	}
}