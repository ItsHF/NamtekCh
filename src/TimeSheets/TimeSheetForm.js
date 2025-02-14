import React, { useState } from 'react';
import axios from 'axios';

const TimesheetForm = ({ employeeId }) => {
  const [timesheet, setTimesheet] = useState({
    employeeId: employeeId,
    startTime: '',
    endTime: '',
    summary: '',
  });

  const [errors, setErrors] = useState([]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setTimesheet((prevTimesheet) => ({
      ...prevTimesheet,
      [name]: value,
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      const response = await axios.post('https://localhost:7035/api/Timesheet', timesheet, {
        headers: { 'Content-Type': 'application/json' },
      });
      alert('Timesheet created successfully!');
    } catch (err) {
      setErrors(err.response?.data?.errors || []);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <div>
        <label>Start Time:</label>
        <input
          type="datetime-local"
          name="startTime"
          value={timesheet.startTime}
          onChange={handleChange}
          required
        />
      </div>
      <div>
        <label>End Time:</label>
        <input
          type="datetime-local"
          name="endTime"
          value={timesheet.endTime}
          onChange={handleChange}
          required
        />
      </div>
      <div>
        <label>Summary:</label>
        <textarea
          name="summary"
          value={timesheet.summary}
          onChange={handleChange}
          required
        ></textarea>
      </div>
      <button type="submit">Submit</button>
      {errors.length > 0 && (
        <div>
          <ul>
            {errors.map((error, index) => (
              <li key={index}>{error}</li>
            ))}
          </ul>
        </div>
      )}
    </form>
  );
};

export default TimesheetForm;
