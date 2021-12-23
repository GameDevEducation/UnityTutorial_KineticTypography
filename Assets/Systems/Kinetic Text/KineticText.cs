using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class KineticText : MonoBehaviour
{
    [SerializeField] float MeshDepth = 0.05f;
    [SerializeField] bool BeginFalling = false;
    [SerializeField] float FallTorque = 10f;

    [SerializeField] float ExplosionForce = 20f;
    [SerializeField] Transform ExplosionLocation;
    [SerializeField] float ExplosionRadius = 10f;
    [SerializeField] float ExplosionUpwardsModifier = 0.2f;
    [SerializeField] ForceMode ExplosionForceMode = ForceMode.VelocityChange;

    class KineticCharacter
    {
        public Rigidbody LinkedRB;
    }

    TMP_Text LinkedText;

    bool TextPrepared = false;
    List<KineticCharacter> KineticCharacters = new List<KineticCharacter> ();

    private void Awake()
    {
        LinkedText = GetComponent<TMP_Text>();
    }

    // Start is called before the first frame update
    void Start()
    {
        PrepareText();
    }

    // Update is called once per frame
    void Update()
    {
        if (!TextPrepared)
            PrepareText();

        if (TextPrepared && BeginFalling)
        {
            BeginFalling = false;
            PerformFall();
        }
    }

    void PrepareText()
    {
        // loop through all of the characters
        int childIndex = 0;
        for (int characterIndex = 0; characterIndex < LinkedText.textInfo.characterInfo.Length; characterIndex++)
        {
            var charInfo = LinkedText.textInfo.characterInfo[characterIndex];

            // if we're at the end then break
            if (charInfo.character == '\0')
                break;

            // skip whitespace
            if (char.IsWhiteSpace(charInfo.character))
            {
                continue;
            }

            var childObject = transform.GetChild(childIndex).gameObject;
            AddKineticCharacter(charInfo, childObject);

            ++childIndex;
        }

        TextPrepared = KineticCharacters.Count > 0;
    }

    void AddKineticCharacter(TMP_CharacterInfo character, GameObject childGO)
    {
        // calculate the centre position
        Vector3 centre = (character.bottomLeft + character.topRight) / 2f;

        // reposition the character
        childGO.transform.localPosition = centre;

        // recentre the mesh
        TMP_SubMesh characterMesh = childGO.GetComponent<TMP_SubMesh>();
        Vector3[] vertices = characterMesh.mesh.vertices;
        for (int index = 0; index < vertices.Length; index++)
            vertices[index] -= centre;
        characterMesh.mesh.SetVertices(vertices);

        // setup the collider
        BoxCollider collider = childGO.AddComponent<BoxCollider>();
        Vector3 size = character.topRight - character.bottomLeft;
        size.z = MeshDepth;
        collider.center = Vector3.zero;
        collider.size = size;

        // add the rigid body
        Rigidbody characterRB = childGO.AddComponent<Rigidbody>();
        characterRB.isKinematic = true;

        KineticCharacters.Add(new KineticCharacter() { LinkedRB = characterRB });
    }

    void PerformFall()
    {
        foreach (var character in KineticCharacters)
        {
            character.LinkedRB.isKinematic = false;

            character.LinkedRB.AddTorque(Random.Range(-FallTorque, FallTorque),
                                         Random.Range(-FallTorque, FallTorque),
                                         Random.Range(-FallTorque, FallTorque));

            character.LinkedRB.AddExplosionForce(ExplosionForce, 
                                                 ExplosionLocation.position, 
                                                 ExplosionRadius,
                                                 ExplosionUpwardsModifier,
                                                 ExplosionForceMode);
        }
    }
}
