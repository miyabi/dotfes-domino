using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class SceneManager : MonoBehaviour {
    public List<GameObject> dominoPrefabs = new List<GameObject>();
    private List<GameObject> dominos = new List<GameObject>();

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.touchCount >= 1) {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended) {
                var ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit raycastHit;
                if (Physics.Raycast(ray, out raycastHit, 1000, 1<<LayerMask.NameToLayer("Domino"))) {
                    AddForceToDomino(raycastHit.transform.gameObject, ray.direction * 30.0f);
                    return;
                }

                var viewportPoint = Camera.main.ScreenToViewportPoint(touch.position);
                ARPoint arPoint = new ARPoint { x = viewportPoint.x, y = viewportPoint.y };

                ARHitTestResultType[] resultTypes = {
                    ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent, 
                    ARHitTestResultType.ARHitTestResultTypeHorizontalPlane, 
                    ARHitTestResultType.ARHitTestResultTypeFeaturePoint,
                };

                foreach (var resultType in resultTypes) {
                    var results = UnityARSessionNativeInterface.GetARSessionNativeInterface()
                        .HitTest(arPoint, resultType);
                    if (results.Count >= 1) {
                        var result = results[0];
                        if (result.anchorIdentifier != null) {
                            var position = UnityARMatrixOps.GetPosition(result.worldTransform);
                            CreateDomino(position);
                            break;
                        }
                    }
                }
            }
        }
	}

    void CreateDomino(Vector3 position) {
        var dominoPrefab = dominoPrefabs[Mathf.FloorToInt(Random.Range(0, dominoPrefabs.Count))];
        var domino = GameObject.Instantiate(dominoPrefab);

        domino.transform.position = position;
        domino.transform.eulerAngles = new Vector3 {
            x = 0.0f,
            y = Camera.main.transform.eulerAngles.y + 180.0f,
            z = 0.0f,
        };
        domino.transform.localScale *= 0.1f;
        domino.transform.localPosition += new Vector3 {
            x = 0.0f,
            y = domino.transform.localScale.y * 0.5f + 0.001f,
            z = 0.0f,
        };
        domino.layer = LayerMask.NameToLayer("Domino");

        dominos.Add(domino);
    }

    void AddForceToDomino(GameObject domino, Vector3 force) {
        var rigidBody = domino.GetComponent<Rigidbody>();
        if (rigidBody != null) {
            rigidBody.AddForce(force);
        }
    }

    void OnGUI() {
        if (GUI.Button(new Rect(Screen.width - 150, 0, 150, 100), "Clear")) {
            foreach (var domino in dominos) {
                GameObject.Destroy(domino);
            }
            dominos.RemoveRange(0, dominos.Count);
        }
    }
}
