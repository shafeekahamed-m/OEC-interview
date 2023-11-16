import React, { useState, useEffect } from "react";
import ReactSelect from "react-select";
import { addUserToProcedure, getUserAssignments } from "../../../api/api";

const PlanProcedureItem = ({ procedure, users, planProcedureId }) => {
  const [selectedUsers, setSelectedUsers] = useState(null);

  const handleAssignUserToProcedure = async (e) => {
    setSelectedUsers(e);
    let users = [];
    if (e) {
      users = e.map((u) => u.value);
    }
    await addUserToProcedure(planProcedureId, users);
    console.log(e, planProcedureId, users);
  };

  useEffect(() => {
    const controller = new AbortController();
    const signal = controller.signal;
    (async () => {
      var usersAssignments = await getUserAssignments(planProcedureId, signal);
      setSelectedUsers(
        users.filter((u) => usersAssignments.find((ua) => ua.userId === u.value))
      );
    })();
    return () => {
      controller.abort();
    };
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
      />
    </div>
  );
};

export default PlanProcedureItem;
