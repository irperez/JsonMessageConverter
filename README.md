# JsonMessageConverter
Json Message Converter for Spring.Net's NMS messaging component

[![Build status](https://ci.appveyor.com/api/projects/status/pdxm45xqopckjywp?svg=true)](https://ci.appveyor.com/project/irperez/jsonmessageconverter)

##Why a Json Message Converter?  
The default is using XML for the deserialization of any object in a text message.  While this is fine for most cases, XML is known to be very wordy and large compared to JSON.  The payload size goes down substantially, hence messages move across the wire must faster.

##Design
This component was designed after the basic XMLMessageConverter in the Spring.Messaging.NMS API.  I replaced the XML conversion to JSON conversion using JSON.Net.  Future versions can potentially abstract out the Json serialization as a subcomponent.
