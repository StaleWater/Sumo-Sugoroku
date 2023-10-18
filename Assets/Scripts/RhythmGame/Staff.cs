using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Staff : MonoBehaviour
{
    [SerializeField] Transform notePrefab;
    [SerializeField] Transform acceptGuidePrefab;
    SpriteRenderer spr;
    Transform acceptGuide;
    List<float> notes, notesCopy;
    Transform[] sprites;

    float reactionTimeBEATS;
    float noteVelocityUPB;
    float tempoBPS; 
    float startDelayBEATS;
    float noteAcceptPosX;
    float noteStartPosX;
    float hitToleranceBEATS;
    float missToleranceBEATS;

    int onScreenStart;
    int onScreenEnd;
    bool reading;
    int notesHit;
    int missedIndex;
    KeyCode key;

    UnityAction<bool, bool> onHitAttempt;

    // measured in beats; when notes are X beats behind the accept area, fade their sprites
    float offscreenValue;

    public void Init(KeyCode key, float reactionTimeBEATS, float tempoBPS, 
                    float hitToleranceBEATS, float missToleranceBEATS) {
        StopAllCoroutines();
        if(acceptGuide != null) Destroy(acceptGuide.gameObject);

        spr = GetComponent<SpriteRenderer>();
        this.reactionTimeBEATS = reactionTimeBEATS;
        this.tempoBPS = tempoBPS;
        this.hitToleranceBEATS = hitToleranceBEATS;
        this.missToleranceBEATS = missToleranceBEATS;
        this.key = key;

        noteStartPosX = spr.bounds.min.x + spr.bounds.size.x * 0.9f; 
        noteAcceptPosX = spr.bounds.min.x + spr.bounds.size.x * 0.1f;
        noteVelocityUPB = (noteStartPosX - noteAcceptPosX) / reactionTimeBEATS;

        offscreenValue = -1.0f;
        reading = false;

        SpawnAcceptGuideSprite();
    }

    public void RegisterOnHitAttempt(UnityAction<bool, bool> a) {
        onHitAttempt += a;
    }

    IEnumerator InputLoop() {
        while(reading) {
            if(Input.GetKeyDown(key)) {
                PlayerHitAttempt();
            }
            yield return null;
        }
    }

    void PlayerHitAttempt() {
        if(onScreenStart > onScreenEnd || missedIndex == onScreenStart) return;

        float diff = Mathf.Abs(notes[onScreenStart]);
        bool hit = diff < hitToleranceBEATS;
        bool miss = diff < missToleranceBEATS;
        if(hit) {
            notesHit++;
            StartCoroutine(DestroyNote(sprites[onScreenStart], true));
            onScreenStart++;
        }
        else if (miss) {
            missedIndex = onScreenStart;
        }

        onHitAttempt(hit, miss);

    }

    public void SetSheetMusic(List<float> notes, float startDelayBEATS) {
        this.notesCopy = new List<float>(notes);
        this.startDelayBEATS = startDelayBEATS;
    }

    void SpawnAcceptGuideSprite() {
        acceptGuide = Instantiate(acceptGuidePrefab, transform, true);
        var pos = transform.position;
        pos.x = noteAcceptPosX;
        acceptGuide.position = pos;
    }

    // returns a note sprite spawned at the right edge of the staff
    Transform SpawnNote() {
        Transform note = Instantiate(notePrefab, transform, true);
        var pos = transform.position;
        pos.x = noteStartPosX;
        note.position = pos;

        // do a fade in animation or something
        var fade = note.GetComponent<Fadeable>();
        fade.Hide();
        fade.FadeIn();        

        return note;
    }

    IEnumerator DestroyNote(Transform note, bool hit) {
        var fade = note.GetComponent<Fadeable>();
        bool doneFading = false;
        fade.FadeOut(null, () => {
            doneFading = true;
        });

        if(hit) {
            var ani = note.GetComponent<Animated>();
            var newScale = note.localScale;
            newScale *= 1.5f;
            yield return StartCoroutine(ani.ScaleTo(newScale));
        }

        yield return new WaitUntil(() => doneFading);

        Destroy(note.gameObject);
    }

    public void StartReading() {
        StartCoroutine(Read());
    }


    IEnumerator Read() {
        notesHit = 0;
        missedIndex = -1;
        onScreenStart = 0;
        onScreenEnd = -1;

        int nextNote = 0;
        float waitTime = startDelayBEATS;
        float beatsPassed;

        notes = new List<float>(notesCopy);
        sprites = new Transform[notes.Count];

        reading = true;
        StartCoroutine(InputLoop());

        while(onScreenStart < notes.Count) {
            yield return null;

            beatsPassed = Time.deltaTime * tempoBPS;

            for(int i=onScreenStart; i <= onScreenEnd; i++) {
                notes[i] -= beatsPassed;

                var pos = sprites[i].position;
                pos.x -= beatsPassed * noteVelocityUPB;
                sprites[i].position = pos;
            }

            if(onScreenStart <= onScreenEnd && notes[onScreenStart] < offscreenValue) {
                StartCoroutine(DestroyNote(sprites[onScreenStart], false));
                // register as a miss
                onScreenStart++;
            }

            if(nextNote >= notes.Count) continue;

            waitTime -= beatsPassed;
            if(waitTime < 0.0f) {
                // next note is now on screen
                sprites[nextNote] = SpawnNote();
                waitTime = notes[nextNote];
                notes[nextNote] = reactionTimeBEATS;
                onScreenEnd++;
                nextNote++;
            }

        }

        reading = false;
    }

}
