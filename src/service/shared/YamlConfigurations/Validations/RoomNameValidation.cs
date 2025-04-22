 

namespace YamlConfigurations.Validations
{
    public class RoomNameValidation : IValidationPass
    {
        public void Validate(YamlMultipleChatRooms config, IList<ValidationError> errors)
        {
            if (errors is not IList<ValidationError> errorList)
                throw new System.ArgumentException("errors must be a mutable collection");

            if (config.Rooms != null)
            {
                foreach (var roomPair in config.Rooms)
                {
                    var roomName = roomPair.Key;
                    var roomValue = roomPair.Value;
                    if (!YamlInstanceOfAgentConfig.IsValidRoomName(roomName))
                    {
                        errorList.Add(new ValidationError(
                            $"Room name '{roomName}' is invalid. Names must not contain spaces or any of: < | \\ / >",
                            $"[Room:{roomName}]",
                            roomValue
                        ));
                    }
                }
            }
            // No return, mutate errorList in place
        }
    }
}
