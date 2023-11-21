import React, { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import {
  addProcedureToPlan,
  getPlanProcedures,
  getProcedures,
  getUsers,
} from "../../api/api";
import Layout from "../Layout/Layout";
import ProcedureItem from "./ProcedureItem/ProcedureItem";
import PlanProcedureItem from "./PlanProcedureItem/PlanProcedureItem";

const Plan = () => {
  let { id } = useParams();
  id = parseInt(id);
  const [procedures, setProcedures] = useState([]);
  const [planProcedures, setPlanProcedures] = useState([]);
  const [users, setUsers] = useState([]);

  useEffect(() => {
    const controller = new AbortController();
    const signal = controller.signal;
    (async () => {
      var procedures = await getProcedures(signal);
      var planProcedures = await getPlanProcedures(id, signal);
      var users = await getUsers(signal);

      var userOptions = [];
      users.map((u) => userOptions.push({ label: u.name, value: u.userId }));

      setUsers(userOptions);
      setProcedures(procedures);
      setPlanProcedures(planProcedures);
    })();
    return () => {
      // cancel the request before component unmounts
      controller.abort();
    };
  }, [id]);

  const handleAddProcedureToPlan = async (procedure) => {
    const hasProcedureInPlan = planProcedures.some(
      (p) => p.procedureId === procedure.procedureId
    );
    if (hasProcedureInPlan) return;

    const addedPlanProcedureId = await addProcedureToPlan(
      id,
      procedure.procedureId
    );
    setPlanProcedures((prevState) => {
      return [
        ...prevState,
        {
          planId: id,
          procedureId: procedure.procedureId,
          procedure: {
            procedureId: procedure.procedureId,
            procedureTitle: procedure.procedureTitle,
          },
          //once we add the procedure to a plan set the planProcedureId returned from the api
          planProcedureId: addedPlanProcedureId,
        },
      ];
    });
  };

  return (
    <Layout>
      <div className="container pt-4">
        <div className="d-flex justify-content-center">
          <h2>OEC Interview Frontend</h2>
        </div>
        {users && users.length > 0 && procedures && procedures.length > 0 && (
          <div className="row mt-4">
            <div className="col">
              <div className="card shadow">
                <h5 className="card-header">Repair Plan</h5>
                <div className="card-body">
                  <div className="row">
                    <div className="col">
                      <h4>Procedures</h4>
                      <div>
                        {procedures.map((p) => (
                          <ProcedureItem
                            key={p.procedureId}
                            procedure={p}
                            handleAddProcedureToPlan={handleAddProcedureToPlan}
                            planProcedures={planProcedures}
                          />
                        ))}
                      </div>
                    </div>
                    <div className="col">
                      <h4>Added to Plan</h4>
                      <div>
                        {planProcedures.map((p) => (
                          <PlanProcedureItem
                            key={p.procedure.procedureId}
                            procedure={p.procedure}
                            users={users}
                            planProcedureId={p.planProcedureId}
                          />
                        ))}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
};

export default Plan;
