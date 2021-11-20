﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : Turtle
{
    public static Transform PlayerTransform;
    public static List<FollowerController> followers = new List<FollowerController>();
    public PressurePlate selectedPressurePlate;
    public bool isInPressurePlateTrigger = false;

    private Turtle[] turtles;

    // Start is called before the first frame update
    void Start()
    {
        InitTurtle();
        PlayerTransform = transform;
        turtles = FindObjectsOfType<Turtle>();
    }

    bool checkTurtlesGroundedStatus()
    {
        foreach (var turtle in turtles)
        {
            if (!turtle.isGrounded)
            {
                return false;
            }
        }

        return true;
    }
    
    // FIX: Double tapping 'E' sends two turtles and has a lot of bugs

    // Update is called once per frame
    protected override void Update()
    {
        // Get Input Axis and create vector        
        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Jump
        if (Input.GetButtonDown("Jump"))
        {
            Jump();

            // Follower jump
            float jumpDelayFloat = 0.15f;
            float _jumpDelayFloat = 0.15f;

            foreach (var follower in followers)
            {
                WaitForSeconds jumpDelay = new WaitForSeconds(jumpDelayFloat);
                jumpDelayFloat += _jumpDelayFloat;

                IEnumerator followerJump()
                {
                    yield return jumpDelay;
                    follower.Jump();
                }

                StartCoroutine(followerJump());
            }
        }

        // Assign Follower
        if (Input.GetKeyDown(KeyCode.E))
        {
            // TODO: Check which Pressure plate, and if empty
            //       Also for turtles returning to player
            // FIX:  isFollowing reset             

            // Send first follower to target
            if (selectedPressurePlate != null
                && !selectedPressurePlate.IsOccupied 
                && selectedPressurePlate.Occupier == null
                && followers.Count != 0) // FIX: Can probs make this check less messy somehow
            {
                bool status = checkTurtlesGroundedStatus();
                Debug.Log("Turtles grounded status is: " + status);

                if (checkTurtlesGroundedStatus()) // If all turtles are grounded
                {
                    selectedPressurePlate.IsOccupied = true;

                    followers[0].halfJump(); // Since InitFollow isn't called (maybe it could be?)                

                    followers[0].SetMovementTarget(selectedPressurePlate.FollowPoint, 0.5f);                    
                    //followers[0].isFollowing = false; // This makes things weird

                    /*selectedPressurePlate.isOccupied = true; */// TODO: could be on event?

                    // Remove first follower from List and tell the pressure plate who's occupying it
                    followers[0].FollowerID = 0;
                    followers[0].isFollowingPlayer = false;
                    selectedPressurePlate.Occupier = followers[0];
                    selectedPressurePlate.hasPlayerAction = true;
                    followers.RemoveAt(0);

                    if (followers.Count != 0)
                    {
                        // Send next turtle in queue to follow player
                        followers[0].SetMovementTarget(PlayerTransform, 1.5f);
                    }

                    // Reset Follower IDs
                    int i = 1;
                    foreach (var follower in followers)
                    {
                        follower.FollowerID = i;
                        i++;
                    }
                }
            }


            else if (selectedPressurePlate != null && selectedPressurePlate.IsOccupied)
            {
                if (checkTurtlesGroundedStatus())
                {
                    selectedPressurePlate.IsOccupied = false;
                    selectedPressurePlate.Occupier.InitFollowPlayer();
                    //selectedPressurePlate.Occupier.SetMovementTarget(transform, 2f);
                    selectedPressurePlate.Occupier = null;
                }                
            }
        }

        base.Update();
    }
}