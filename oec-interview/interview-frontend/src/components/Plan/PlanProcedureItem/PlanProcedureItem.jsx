import React, { useState, useEffect } from "react";
import ReactSelect from "react-select";
import {
  addUserToProcedure,
  getUserAssignments,
  removeUserFromProcedure,
} from "../../../api/api";

const PlanProcedureItem = ({ procedure, users, planProcedureId }) => {
  const [selectedUsers, setSelectedUsers] = useState(null);

  const handleAssignUserToProcedure = async (e) => {
    //first set the state with the selected data
    setSelectedUsers(e);
    //add the selected users to the below list
    let users = [];
    if (e) {
      users = e.map((u) => u.value);
    }
    if (users && selectedUsers) {
      //get the function which needs to be called
      //eg. if the selected count is x , the current possibilities are x+1,x-1 or x-x which is clearing the field
      //for cases x-1 and x-x call remove
      //for x+1 cases call add
      const userActionApiCall =
        users.length < selectedUsers.length
          ? removeUserFromProcedure
          : addUserToProcedure;
      //call the api
      await userActionApiCall(planProcedureId, users);
    }
    console.log(e, planProcedureId, users);
  };

  useEffect(() => {
    // //used to cancel the current request in the event of component unload
    // const controller = new AbortController();
    // const signal = controller.signal;
    (async () => {
      //get user assignments for the planProcedureId and set the state
      var usersAssignments = await getUserAssignments(planProcedureId);
      setSelectedUsers(
        users.filter((u) =>
          usersAssignments?.find((ua) => ua.userId === u.value)
        )
      );
    })();
    // //useeffect cleanup to cancel the current request
    // return () => {
    //   controller.abort();
    // };
  }, [planProcedureId, users]);

  return (
    <div className="py-2">
      <div>{procedure.procedureTitle}</div>

      <ReactSelect
        className="mt-2"
        placeholder="Select User to Assign"
        isMulti={true}
        options={users}
        value={selectedUsers}
        onChange={(e) => handleAssignUserToProcedure(e)}
        oncl
      />
    </div>
  );
};

export default PlanProcedureItem;
