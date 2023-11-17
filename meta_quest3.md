# Query: Real-Time External Camera Feed with AR Foundation 6.0 on Meta Quest 3

## User Inquiry
**User:** Wooram Son  
**Date:** November 10, 2023

### Description
Wooram Son inquires about the feasibility of capturing real-time external camera footage using Meta Quest 3 with OpenXR and AR Foundation 6.0. Despite documentation suggesting support for Camera Feature in Meta Quest (OpenXR), challenges were faced when running the CpuImages Scene from the AR Foundation Samples.

#### Testing Environment
- Unity Version: 2023.2.0b12
- AR Foundation Samples Version: 6.0.0-pre.4 release
- Test Scene: Scenes/Camera/CpuImages
- Documentation Reference: [Unity Docs](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.0/manual/index.html#top)
- AR Foundation Samples: [GitHub](https://github.com/Unity-Technologies/arfoundation-samples)

## Response from Unity
**Responder:** Gordon, Customer Experience Advisor  
**Date:** November 10, 2023

### Initial Response
Gordon, from Unity's customer experience team, acknowledges the inquiry and commits to consulting with the technical team.

### Follow-up Response
Gordon conveys the technical team's feedback:
- Meta does not allow app access to the camera feed.
- CPU images are not supported on Quest.
- The `supportsCameraImage` flag on the ARCameraManager's subsystem descriptor can be used to check platform support for CPU images.
- Passthrough functionality is achieved using Meta's API for composition layer submission in the render stack, but AR Foundation does not have read access to the image's pixel data.


## Response from Meta
**Responder:** Hakim, Meta Store Support
**Date:** November 14, 2023

### Summary
Confirmation that Meta Quest 3 does not currently support exporting information for external use. While developer tools might offer a workaround, official support for such implementations is not available. Users are encouraged to share ideas and feedback on the Meta Quest Community forums.



## Conclusion
The current implementation of AR Foundation 6.0 on Meta Quest 3 does not support access to real-time external camera feeds due to platform limitations.



# The original message is attached below.

### Issue: Real-Time External Camera Feed with AR Foundation 6.0 on Meta Quest 3

---

**Wooram Son** 

Hello, I'm interested in whether it's possible to capture real-time external camera footage using Meta Quest 3 in combination with OpenXR and AR Foundation 6.0.

According to the documentation located here: [Unity3D AR Foundation Docs](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.0/manual/index.html#top), it mentions that Camera Feature is supported in Meta Quest (OpenXR).

In light of this, I attempted to run the CpuImages Scene from the AR Foundation Samples, but I encountered difficulties in obtaining camera information or video feeds. Has anyone in the community successfully managed to retrieve camera feed from Meta Quest 3 using the AR Foundation Camera Feature?

For reference, here are the specifics of my testing environment:
- Unity Version: 2023.2.0b12
- AR Foundation Samples Version: 6.0.0-pre.4 release
- [AR Foundation Samples](https://github.com/Unity-Technologies/arfoundation-samples)
- Test Scene: Scenes/Camera/CpuImages

---

**Gordon (Customer Experience Advisor)** 

Hey Wooram,

Thanks for getting in touch. I'm Gordon from the customer experience team at Unity.

I'm thrilled to hear about your interest in exploring how to receive real-time external camera feed with AR Foundation 6.0 on Meta Quest 3. That's some cool stuff you're working on – keep it up!

While I'm not a technical expert, I've forwarded your query to the right team for their expertise. Please be patient as we await their response, and I'll update you once I hear back from them, alright?

In the meantime, I took the liberty of internally searching for information on your query. It seems that access to external cameras on the Meta Quest is currently not officially supported. For XR experiences, the Quest platform, even with AR Foundation or OpenXR, does not currently provide direct access to the camera feed. However, I'm not entirely certain about this, so we'll need to wait for confirmation from the dedicated team.

Thanks a bunch for your understanding!
Best regards,
Gordon
Customer Experience Advisor

---

**Gordon (Customer Experience Advisor)** 

Hey Wooram,

Thanks for your patience. I've received an update from the dedicated team regarding your query. Here's what they shared:

Meta does not allow apps to access the camera feed. CPU images are not supported on Quest. Users can check whether a platform supports CPU images by reading the `supportsCameraImage` flag on their ARCameraManager's subsystem descriptor. [Link to documentation](#)

Passthrough works because we use Meta's API to submit a composition layer into the render stack, but at no point does AR Foundation have read access to the pixel data contained in the image.

I hope this information is helpful.
Let me know if you have any further concerns. Thank you!

Best regards,
Gordon
Customer Experience Advisor
