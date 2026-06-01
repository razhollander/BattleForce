1. **Analyze `MatchPlayerView.cs` and `MatchPlayerController.cs`**: In `MatchPlayerView`, there is a method `SetIsLockOnHeartSightShown(bool isShown)`. Currently, it only toggles the sight view: `_lockOnHeartSightView.SetIsShown(isShown);`. We also need to update the player's eyes so that when `isShown` is true, the player has "angry eyes".

2. **Update `PlayerEyesView.cs`**:
   - `PlayerEyesView` already has `MakeAngryForShortDuration(CancellationToken cancellationToken)` which sets angry eyes for a short duration.
   - We need a way to permanently toggle the angry eyes ON when the lock-on sight is shown, and OFF when it's hidden (reverting to normal eyes). Or at least manage this state without a short-duration timeout.
   - We should add a new method in `PlayerEyesView`: `SetAngryEyesToggledState(bool isAngry)`. This method should set `_angryLeftEye.TrySetActive(isAngry);`, `_angryRightEye.TrySetActive(isAngry);`, `_leftEye.gameObject.TrySetActive(!isAngry);`, `_rightEye.gameObject.TrySetActive(!isAngry);`.
   - *Wait, `MakeAngryForShortDuration` also disables the spinned state (`DisableSpinned();`). Let's see how they interact. If the player is spinned, spinned eyes might have priority. Also, if they are already angry from shooting (`MakeAngryForShortDuration`), and then we toggle LockOnSight, how does it interact?*
   - Let's look at `PlayerEyesView.cs`.

   ```csharp
   public void MakeAngryForShortDuration(CancellationToken cancellationToken)
   {
       DisableSpinned();
       _angryEyesCancellationTokenSource?.Cancel();
       _angryEyesCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
       SetAngryForShortDurationAsync(_angryEyesCancellationTokenSource.Token).Forget();
   }
   ```
   If we have `SetIsLockOnHeartSightShown(bool isShown)` turning eyes permanently angry, we probably need a field `_isLockOnAngry` or something similar, so that when `SetAngryForShortDurationAsync` finishes, it checks `_isLockOnAngry` before reverting to normal eyes. Or we just keep it simple: `SetLockOnAngryState(bool isAngry)`. Wait, we just need to modify `PlayerEyesView.cs` and `MatchPlayerView.cs`.

3. **Plan**:
   - Add `SetIsLockOnAngry(bool isAngry)` to `PlayerEyesView.cs`. This will update a field `_isLockOnAngry = isAngry`. Then call `UpdateEyeState()`.
   - Update `SetAngryForShortDurationAsync` to not just blindly turn off angry eyes if `_isLockOnAngry` is true. Let's make it so that it always sets the angry game objects to active if either short duration angry is active OR lock-on angry is active.

Let's check `PlayerEyesView.cs` again to see what's the best way to implement this state machine for eyes.
